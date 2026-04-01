using COTHUYPRO.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace COTHUYPRO.Controllers;

[Authorize(Policy = "InstructorOnly")]
public class InstructorController : Controller
{
    private readonly TrainingContext _context;

    public InstructorController(TrainingContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> GenerateLessonContent([FromBody] AiRequestMsg req)
    {
        if (string.IsNullOrWhiteSpace(req.Topic)) return BadRequest();
        var aiService = HttpContext.RequestServices.GetService(typeof(COTHUYPRO.Services.IAiService)) as COTHUYPRO.Services.IAiService;
        var content = await aiService!.GenerateLessonContentAsync(req.Topic);
        return Json(new { success = true, content });
    }

    public IActionResult Index(int? yearId, int? semesterId, int? blockId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var classesQuery = _context.Classes
            .Include(c => c.Course)
            .Include(c => c.Room)
            .Include(c => c.Block).ThenInclude(b => b!.Semester).ThenInclude(s => s!.Year)
            .Include(c => c.ClassStudents).ThenInclude(cs => cs.User)
            .Where(c => c.InstructorId == userId);

        if (blockId.HasValue) classesQuery = classesQuery.Where(c => c.BlockId == blockId);
        else if (semesterId.HasValue) classesQuery = classesQuery.Where(c => c.Block!.SemesterId == semesterId);
        else if (yearId.HasValue) classesQuery = classesQuery.Where(c => c.Block!.Semester!.YearId == yearId);

        var classes = classesQuery.ToList();

        var courses = _context.CourseInstructors
            .Include(ci => ci.Course).ThenInclude(c => c!.Level)
            .Include(ci => ci.Course).ThenInclude(c => c!.Category)
            .Where(ci => ci.UserId == userId)
            .Select(ci => ci.Course!)
            .Distinct()
            .ToList();

        var students = _context.ClassStudents
            .Include(cs => cs.User)
            .Include(cs => cs.Class).ThenInclude(cls => cls!.Course)
            .Where(cs => cs.Class != null && cs.Class.InstructorId == userId)
            .Select(cs => cs.User!)
            .Distinct()
            .ToList();

        var vm = new InstructorDashboardVm
        {
            Courses = courses,
            Classes = classes,
            Students = students
        };

        ViewBag.AcademicYears = _context.AcademicYears.ToList();
        ViewBag.Semesters = _context.Semesters.Include(s => s.Year).ToList();
        ViewBag.Blocks = _context.Blocks.Include(b => b.Semester).ThenInclude(s => s!.Year).ToList();

        ViewBag.SelectedYear = yearId;
        ViewBag.SelectedSemester = semesterId;
        ViewBag.SelectedBlock = blockId;

        return View(vm);
    }

    [HttpGet]
    public IActionResult CourseLessons(int courseId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (!_context.CourseInstructors.Any(ci => ci.CourseId == courseId && ci.UserId == userId))
            return Forbid();

        var course = _context.Courses.FirstOrDefault(c => c.Id == courseId);
        if (course == null) return NotFound();

        var chapters = _context.CourseChapters
            .Where(c => c.CourseId == courseId)
            .OrderBy(c => c.OrderIndex)
            .Select(c => new CourseChapterRow
            {
                Id = c.Id,
                Title = c.Title,
                OrderIndex = c.OrderIndex,
                Lessons = _context.Lessons
                    .Where(l => l.ChapterId == c.Id)
                    .OrderBy(l => l.OrderIndex)
                    .Select(l => new LessonRow
                    {
                        Id = l.Id,
                        Title = l.Title,
                        OrderIndex = l.OrderIndex,
                        DocumentUrl = l.DocumentUrl,
                        Status = l.Status
                    })
                    .ToList()
            })
            .ToList();

        var orphanLessons = _context.Lessons
            .Where(l => l.CourseId == courseId && l.ChapterId == null)
            .OrderBy(l => l.OrderIndex)
            .Select(l => new LessonRow
            {
                Id = l.Id,
                Title = l.Title,
                OrderIndex = l.OrderIndex,
                DocumentUrl = l.DocumentUrl,
                Status = l.Status
            })
            .ToList();

        if (orphanLessons.Any())
        {
            chapters.Add(new CourseChapterRow
            {
                Id = 0,
                Title = "Bài học chưa phân mục",
                OrderIndex = 999,
                Lessons = orphanLessons
            });
        }

        return View("Lessons/CourseLessons", new CourseLessonsVm
        {
            CourseId = course.Id,
            CourseName = course.CourseName,
            Chapters = chapters
        });
    }

    [HttpGet]
    public IActionResult CreateLesson(int? courseId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var courses = _context.CourseInstructors
            .Where(ci => ci.UserId == userId)
            .Select(ci => ci.Course)
            .ToList();

        ViewBag.Courses = courses;
        return View("Lessons/Create", new LessonFormVm { CourseId = courseId ?? 0 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateLesson(LessonFormVm model)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (!_context.CourseInstructors.Any(ci => ci.CourseId == model.CourseId && ci.UserId == userId))
            return Forbid();

        if (!ModelState.IsValid)
        {
            ViewBag.Courses = _context.CourseInstructors.Where(ci => ci.UserId == userId).Select(ci => ci.Course).ToList();
            return View("Lessons/Create", model);
        }

        string? docUrl = model.DocumentUrl;
        if (model.DocumentFile != null && model.DocumentFile.Length > 0)
        {
            var fileName = Guid.NewGuid() + Path.GetExtension(model.DocumentFile.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "lessons", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
            {
                model.DocumentFile.CopyTo(stream);
            }
            docUrl = "/uploads/lessons/" + fileName;
        }

        _context.Lessons.Add(new Lesson
        {
            Title = model.Title,
            CourseId = model.CourseId,
            OrderIndex = model.OrderIndex,
            DocumentUrl = docUrl,
            VideoUrl = model.VideoUrl,
            Content = model.Content,
            Duration = model.Duration,
            IsFreePreview = model.IsFreePreview,
            IsPublished = model.IsPublished,
            Status = "Pending",
            CreatedByUserId = userId
        });
        _context.SaveChanges();
        TempData["Message"] = "Đã gửi bài giảng. Vui lòng đợi Admin phê duyệt.";
        return RedirectToAction(nameof(CourseLessons), new { courseId = model.CourseId });
    }

    [HttpGet]
    public IActionResult EditLesson(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var lesson = _context.Lessons.FirstOrDefault(l => l.Id == id);
        if (lesson == null) return NotFound();

        if (!_context.CourseInstructors.Any(ci => ci.CourseId == lesson.CourseId && ci.UserId == userId))
            return Forbid();

        ViewBag.Courses = _context.CourseInstructors.Where(ci => ci.UserId == userId).Select(ci => ci.Course).ToList();

        var vm = new LessonFormVm
        {
            Id = lesson.Id,
            CourseId = lesson.CourseId,
            Title = lesson.Title,
            OrderIndex = lesson.OrderIndex,
            DocumentUrl = lesson.DocumentUrl,
            VideoUrl = lesson.VideoUrl,
            Content = lesson.Content,
            Duration = lesson.Duration,
            IsFreePreview = lesson.IsFreePreview,
            IsPublished = lesson.IsPublished
        };
        return View("Lessons/Edit", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditLesson(int id, LessonFormVm model)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (!_context.CourseInstructors.Any(ci => ci.CourseId == model.CourseId && ci.UserId == userId))
            return Forbid();

        if (!ModelState.IsValid)
        {
            ViewBag.Courses = _context.CourseInstructors.Where(ci => ci.UserId == userId).Select(ci => ci.Course).ToList();
            return View("Lessons/Edit", model);
        }

        var lesson = _context.Lessons.FirstOrDefault(l => l.Id == id);
        if (lesson == null) return NotFound();

        string? docUrl = lesson.DocumentUrl;
        if (model.DocumentFile != null && model.DocumentFile.Length > 0)
        {
            var fileName = Guid.NewGuid() + Path.GetExtension(model.DocumentFile.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "lessons", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            using (var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create))
            {
                model.DocumentFile.CopyTo(stream);
            }
            docUrl = "/uploads/lessons/" + fileName;
        }

        lesson.Title = model.Title;
        lesson.CourseId = model.CourseId;
        lesson.OrderIndex = model.OrderIndex;
        lesson.DocumentUrl = docUrl;
        lesson.VideoUrl = model.VideoUrl;
        lesson.Content = model.Content;
        lesson.Duration = model.Duration;
        lesson.IsFreePreview = model.IsFreePreview;
        lesson.IsPublished = model.IsPublished;

        lesson.Status = "Pending";
        lesson.UpdatedByUserId = userId;
        lesson.UpdatedAt = DateTime.Now;

        _context.SaveChanges();
        TempData["Message"] = "Đã cập nhật bài giảng. Trạng thái đã chuyển về Chờ Duyệt.";

        return RedirectToAction(nameof(CourseLessons), new { courseId = model.CourseId });
    }

    public IActionResult Exams()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var assignedCourseIds = _context.CourseInstructors
            .Where(ci => ci.UserId == userId)
            .Select(ci => ci.CourseId)
            .ToList();

        var exams = _context.Exams
            .Include(e => e.Course)
            .Where(e => assignedCourseIds.Contains(e.CourseId))
            .OrderByDescending(e => e.CreatedAt)
            .ToList();

        return View("Exams/Index", exams);
    }

    [HttpGet]
    public IActionResult CreateExam(int? courseId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        ViewBag.Courses = _context.CourseInstructors
            .Where(ci => ci.UserId == userId)
            .Select(ci => ci.Course)
            .ToList();

        return View("Exams/Create", new Exam { CourseId = courseId ?? 0 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateExam(Exam model)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (!_context.CourseInstructors.Any(ci => ci.CourseId == model.CourseId && ci.UserId == userId))
            return Forbid();

        model.Status = "Pending";
        model.CreatedByUserId = userId;
        model.CreatedAt = DateTime.Now;

        _context.Exams.Add(model);
        _context.SaveChanges();

        TempData["Message"] = "Đã gửi Quiz. Vui lòng đợi Admin phê duyệt.";
        return RedirectToAction(nameof(Exams));
    }

    public IActionResult Assignments()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var assignedCourseIds = _context.CourseInstructors
            .Where(ci => ci.UserId == userId)
            .Select(ci => ci.CourseId)
            .ToList();

        var assignments = _context.Assignments
            .Include(a => a.Course)
            .Where(a => assignedCourseIds.Contains(a.CourseId))
            .OrderByDescending(a => a.CreatedAt)
            .ToList();

        return View("Assignments/Index", assignments);
    }

    [HttpGet]
    public IActionResult CreateAssignment(int? courseId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        ViewBag.Courses = _context.CourseInstructors
            .Where(ci => ci.UserId == userId)
            .Select(ci => ci.Course)
            .ToList();

        return View("Assignments/Create", new Assignment { CourseId = courseId ?? 0 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateAssignment(Assignment model)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (!_context.CourseInstructors.Any(ci => ci.CourseId == model.CourseId && ci.UserId == userId))
            return Forbid();

        model.Status = "Pending";
        model.CreatedByUserId = userId;
        model.CreatedAt = DateTime.Now;

        _context.Assignments.Add(model);
        _context.SaveChanges();

        TempData["Message"] = "Đã gửi Bài tập. Vui lòng đợi Admin phê duyệt.";
        return RedirectToAction(nameof(Assignments));
    }

    public IActionResult StudentList(int classId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var cls = _context.Classes
            .Include(c => c.Course)
            .Include(c => c.Block).ThenInclude(b => b!.Semester).ThenInclude(s => s!.Year)
            .Include(c => c.ClassStudents).ThenInclude(cs => cs.User)
            .FirstOrDefault(c => c.Id == classId && c.InstructorId == userId);

        if (cls == null) return Forbid();

        var classHistory = new ClassDetailVm
        {
            ClassInfo = cls,
            Lessons = _context.Lessons
                .Where(l => l.CourseId == cls.CourseId)
                .OrderBy(l => l.OrderIndex)
                .ToList(),
            Exams = _context.Exams
                .Where(e => e.CourseId == cls.CourseId)
                .OrderByDescending(e => e.CreatedAt)
                .ToList(),
            Assignments = _context.Assignments
                .Where(a => a.CourseId == cls.CourseId)
                .OrderByDescending(a => a.CreatedAt)
                .ToList()
        };

        foreach (var student in cls.ClassStudents)
        {
            var studentHistory = new ClassStudentHistoryVm
            {
                StudentId = student.UserId,
                StudentName = student.User?.FullName ?? "Unknown",
                StudentCode = student.User?.StudentCode ?? string.Empty
            };

            studentHistory.ExamResults = _context.ExamResults
                .Include(r => r.Attempt).ThenInclude(a => a!.Exam)
                .Where(r => r.Attempt!.UserId == student.UserId && r.Attempt.Exam!.CourseId == cls.CourseId)
                .OrderByDescending(r => r.Attempt.StartedAt)
                .Select(r => new ClassExamResultVm
                {
                    ExamId = r.Attempt.ExamId,
                    ExamTitle = r.Attempt.Exam!.Title,
                    Score = r.Score,
                    TotalQuestions = r.TotalQuestions,
                    AttemptDate = r.Attempt.StartedAt,
                    IsPassed = r.Score >= (r.Attempt.Exam.PassingScore / 10.0)
                })
                .ToList();

            studentHistory.AssignmentSubmissions = _context.AssignmentSubmissions
                .Include(s => s.Assignment)
                .Where(s => s.StudentId == student.UserId && s.Assignment!.CourseId == cls.CourseId)
                .OrderByDescending(s => s.SubmittedAt)
                .Select(s => new ClassAssignmentSubmissionVm
                {
                    SubmissionId = s.Id,
                    AssignmentTitle = s.Assignment!.Title,
                    Score = s.Score,
                    Feedback = s.Feedback ?? string.Empty,
                    SubmittedAt = s.SubmittedAt,
                    Status = s.Score.HasValue ? "Graded" : "Pending"
                })
                .ToList();

            classHistory.StudentHistories.Add(studentHistory);
        }

        return View(classHistory);
    }
}
