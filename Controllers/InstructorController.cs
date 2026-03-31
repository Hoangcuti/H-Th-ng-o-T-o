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

    public IActionResult Index()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var classes = _context.Classes
            .Include(c => c.Course)
            .Include(c => c.Room)
            .Include(c => c.Status)
            .Include(c => c.Schedules)
            .Include(c => c.ClassStudents).ThenInclude(cs => cs.User)
            .Where(c => c.InstructorId == userId)
            .ToList();

        var courses = _context.CourseInstructors
            .Include(ci => ci.Course).ThenInclude(c => c.Level)
            .Include(ci => ci.Course).ThenInclude(c => c.Category)
            .Where(ci => ci.UserId == userId)
            .Select(ci => ci.Course!)
            .Distinct()
            .ToList();

        var students = _context.ClassStudents
            .Include(cs => cs.User)
            .Include(cs => cs.Class).ThenInclude(cls => cls.Course)
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

        // Danh sách các khóa học mà giảng viên chưa được gán và chưa đăng ký (hoặc đã bị từ chối)
        var assignedCourseIds = courses.Select(c => c.Id).ToList();
        var pendingApplicationCourseIds = _context.InstructorCourseApplications
            .Where(a => a.UserId == userId && a.Status == "Pending")
            .Select(a => a.CourseId)
            .ToList();

        // Lấy tất cả các khóa học ngoại trừ những khóa giảng viên HIỆN ĐANG dạy hoặc ĐANG CHỜ duyệt
        ViewBag.AvailableCourses = _context.Courses
            .Include(c => c.Instructors).ThenInclude(ci => ci.User)
            .Include(c => c.Category)
            .Where(c => !assignedCourseIds.Contains(c.Id) && !pendingApplicationCourseIds.Contains(c.Id))
            .ToList();

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ApplyForCourse(int courseId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        // Kiểm tra xem đã đăng ký chưa
        var exists = _context.InstructorCourseApplications
            .Any(a => a.UserId == userId && a.CourseId == courseId && a.Status == "Pending");

        if (!exists)
        {
            _context.InstructorCourseApplications.Add(new InstructorCourseApplication
            {
                UserId = userId,
                CourseId = courseId,
                ApplyDate = DateTime.Now,
                Status = "Pending"
            });
            _context.SaveChanges();
            TempData["Message"] = "Đã gửi đơn đăng ký giảng dạy. Vui lòng chờ Admin phê duyệt.";
        }
        else
        {
            TempData["Error"] = "Bạn đã gửi đơn đăng kí cho khóa học này rồi.";
        }

        return RedirectToAction(nameof(Index));
    }

    // ==========================================
    // QUẢN LÝ LỘ TRÌNH (BÀI GIẢNG) CHO GIẢNG VIÊN
    // ==========================================

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

    // ==========================================
    // QUẢN LÝ QUIZ (BÀI KIỂM TRA)
    // ==========================================

    [HttpGet]
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

    // ==========================================
    // QUẢN LÝ BÀI TẬP (ASSIGNMENT)
    // ==========================================

    [HttpGet]
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
}
