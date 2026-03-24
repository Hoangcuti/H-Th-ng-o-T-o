using COTHUYPRO.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COTHUYPRO.Services;

namespace COTHUYPRO.Controllers;

[Authorize(Policy = "AdminOnly")]
public class AdminController : Controller
{
    private readonly TrainingContext _context;
    private readonly IAiService _aiService;

    public AdminController(TrainingContext context, IAiService aiService)
    {
        _context = context;
        _aiService = aiService;
    }

    public IActionResult Index()
    {
        var stats = new AdminDashboardVm
        {
            UserCount = _context.Users.Count(),
            CourseCount = _context.Courses.Count(),
            ClassCount = _context.Classes.Count(),
            ExamCount = _context.Exams.Count(),
            PendingCertificates = _context.UserCertificates.Count()
        };

        var latestUsers = _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .OrderByDescending(u => u.CreatedAt)
            .Take(8)
            .ToList();

        var latestCourses = _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Level)
            .OrderByDescending(c => c.Id)
            .Take(6)
            .ToList();

        var learnerByCourse = _context.LearningProgress
            .GroupBy(lp => lp.CourseId)
            .Select(g => new { CourseId = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .Take(5)
            .ToList();

        var courseLookup = _context.Courses.ToDictionary(c => c.Id, c => c.CourseName);
        const decimal MOCK_PRICE = 500000m; 

        var topCourses = learnerByCourse
            .Select(g => new RevenueCourseVm
            {
                CourseName = courseLookup.TryGetValue(g.CourseId, out var name) ? name : $"Course {g.CourseId}",
                Learners = g.Count,
                Revenue = g.Count * MOCK_PRICE
            })
            .ToList();

        var vm = new AdminIndexViewModel
        {
            Stats = stats,
            RecentUsers = latestUsers,
            RecentCourses = latestCourses,
            TopCourses = topCourses
        };
        return View(vm);
    }

    public IActionResult Courses()
    {
        var data = _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Level)
            .Include(c => c.Instructors).ThenInclude(i => i.User)
            .Include(c => c.Classes)
            .Include(c => c.LearningProgresses)
            .Select(c => new AdminCourseRow
            {
                Course = c,
                CategoryName = c.Category != null ? c.Category.Name : "Chua phan loai",
                LevelName = c.Level != null ? c.Level.Name : "Khong cap do",
                ClassCount = c.Classes.Count,
                LearnerCount = c.LearningProgresses.Count,
                Instructors = c.Instructors.Select(i => i.User!.FullName).ToList()
            })
            .ToList();

        return View("Courses/Index", new CoursesPageVm { Courses = data });
    }

    [HttpGet]
    public IActionResult CreateCourse()
    {
        ViewBag.Categories = _context.CourseCategories.ToList();
        ViewBag.Levels = _context.CourseLevels.ToList();
        return View("Courses/Create", new CourseFormVm());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateCourse(CourseFormVm model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = _context.CourseCategories.ToList();
            ViewBag.Levels = _context.CourseLevels.ToList();
            return View("Courses/Create", model);
        }

        var course = new Course
        {
            CourseName = model.CourseName,
            CategoryId = model.CategoryId,
            LevelId = model.LevelId
        };
        _context.Courses.Add(course);
        _context.SaveChanges();
        TempData["Message"] = "Đã tạo khóa học.";
        return RedirectToAction(nameof(Courses));
    }

    [HttpGet]
    public IActionResult EditCourse(int id)
    {
        var course = _context.Courses.FirstOrDefault(c => c.Id == id);
        if (course == null) return NotFound();
        ViewBag.Categories = _context.CourseCategories.ToList();
        ViewBag.Levels = _context.CourseLevels.ToList();
        var vm = new CourseFormVm
        {
            Id = course.Id,
            CourseName = course.CourseName,
            CategoryId = course.CategoryId,
            LevelId = course.LevelId
        };
        return View("Courses/Edit", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditCourse(CourseFormVm model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Categories = _context.CourseCategories.ToList();
            ViewBag.Levels = _context.CourseLevels.ToList();
            return View("Courses/Edit", model);
        }
        var course = _context.Courses.FirstOrDefault(c => c.Id == model.Id);
        if (course == null) return NotFound();
        course.CourseName = model.CourseName;
        course.CategoryId = model.CategoryId;
        course.LevelId = model.LevelId;
        _context.SaveChanges();
        TempData["Message"] = "Đã cập nhật khóa học.";
        return RedirectToAction(nameof(Courses));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteCourse(int id)
    {
        var course = _context.Courses.FirstOrDefault(c => c.Id == id);
        if (course == null) return NotFound();
        _context.Courses.Remove(course);
        _context.SaveChanges();
        TempData["Message"] = "Đã xóa khóa học.";
        return RedirectToAction(nameof(Courses));
    }

    public IActionResult Accounts()
    {
        var accounts = _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Select(u => new UserAccountVm
            {
                User = u,
                Roles = u.UserRoles.Select(r => r.Role!.Name).ToList()
            })
            .ToList();
        var roles = _context.Roles.ToList();
        var categories = _context.CourseCategories.ToList();
        return View("Accounts/Index", new AccountsPageVm { Accounts = accounts, Roles = roles, Categories = categories });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AssignRole(int userId, int roleId, int? categoryId)
    {
        var user = _context.Users
            .Include(u => u.UserRoles)
            .Include(u => u.UserProfiles)
            .FirstOrDefault(u => u.Id == userId);
        var role = _context.Roles.FirstOrDefault(r => r.Id == roleId);
        if (user == null || role == null) return NotFound();

        // simple: mỗi tài khoản 1 vai trò chính
        user.UserRoles.Clear();
        user.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleId });

        if (role.Name == "Student" && categoryId.HasValue)
        {
            var category = _context.CourseCategories.FirstOrDefault(c => c.Id == categoryId.Value);
            var prefix = category != null ? GetPrefix(category.Name) : "STU";
            user.StudentCode = $"{prefix}{user.Id:D4}";
        }

        _context.SaveChanges();

        TempData["Message"] = $"Đã gán quyền {role.Name} cho {user.FullName}";
        return RedirectToAction(nameof(Accounts));
    }

    [HttpGet]
    public IActionResult EditAccount(int id)
    {
        var user = _context.Users.Include(u => u.UserProfiles).FirstOrDefault(u => u.Id == id);
        if (user == null) return NotFound();
        var vm = new EditAccountVm
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.UserProfiles?.FirstOrDefault()?.Phone ?? ""
        };
        return View("Accounts/Edit", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditAccount(EditAccountVm model)
    {
        if (!ModelState.IsValid) return View("Accounts/Edit", model);
        var user = _context.Users.Include(u => u.UserProfiles).FirstOrDefault(u => u.Id == model.Id);
        if (user == null) return NotFound();
        user.FullName = model.FullName;
        user.Email = model.Email;
        var profile = user.UserProfiles?.FirstOrDefault();
        if (profile == null)
            _context.UserProfiles.Add(new UserProfile { UserId = user.Id, Phone = model.Phone });
        else
            profile.Phone = model.Phone;
        _context.SaveChanges();
        TempData["Message"] = "Đã cập nhật tài khoản.";
        return RedirectToAction(nameof(Accounts));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteAccount(int id)
    {
        var user = _context.Users.Include(u => u.UserRoles).FirstOrDefault(u => u.Id == id);
        if (user == null) return NotFound();
        _context.UserRoles.RemoveRange(user.UserRoles);
        var profile = _context.UserProfiles.FirstOrDefault(p => p.UserId == id);
        if (profile != null) _context.UserProfiles.Remove(profile);
        _context.Users.Remove(user);
        _context.SaveChanges();
        TempData["Message"] = "Đã xóa tài khoản.";
        return RedirectToAction(nameof(Accounts));
    }

    private string GetPrefix(string name)
    {
        var lower = name.ToLowerInvariant();
        if (lower.Contains("software")) return "SD0";
        if (lower.Contains("dev")) return "SD0";
        if (lower.Contains("marketing")) return "MT0";
        if (lower.Contains("thiết") || lower.Contains("design") || lower.Contains("đồ họa")) return "TH0";
        if (lower.Contains("hardware") || lower.Contains("hw")) return "HW0";
        return "STU";
    }

    public IActionResult Students()
    {
        var students = _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.Department)
            .Include(u => u.Position)
            .Include(u => u.LearningProgresses).ThenInclude(lp => lp.Course)
            .Where(u => u.UserRoles.Any(r => r.Role!.Name == "Student"))
            .Select(u => new StudentVm
            {
                User = u,
                Department = u.Department != null ? u.Department.Name : "-",
                Position = u.Position != null ? u.Position.Name : "-",
                CompletedCourses = u.LearningProgresses.Count(lp => lp.Percent >= 100),
                StudentCode = !string.IsNullOrEmpty(u.StudentCode) ? u.StudentCode : "Chưa cấp",
                EnrolledCourses = u.LearningProgresses.Where(lp => lp.Course != null).Select(lp => lp.Course!.CourseName).ToList()
            })
            .ToList();
        return View("Students/Index", new StudentsPageVm { Students = students });
    }

    public IActionResult Instructors()
    {
        var instructors = _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.TeachingCourses).ThenInclude(tc => tc.Course)
            .Where(u => u.UserRoles.Any(r => r.Role!.Name == "Instructor"))
            .Select(u => new InstructorVm
            {
                User = u,
                CourseCount = u.TeachingCourses.Count
            })
            .ToList();
        var courses = _context.Courses.ToList();
        return View("Instructors/Index", new InstructorsPageVm { Instructors = instructors, Courses = courses });
    }

    [HttpGet]
    public IActionResult CreateInstructor()
    {
        return View("Instructors/Create", new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateInstructor(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View("Instructors/Create", model);

        var instructorRole = _context.Roles.FirstOrDefault(r => r.Name == "Instructor");
        if (instructorRole == null)
        {
            TempData["Error"] = "Chưa có role Instructor trong hệ thống.";
            return RedirectToAction(nameof(Instructors));
        }

        var user = new User
        {
            Username = model.Username,
            FullName = model.FullName,
            Email = model.Email,
            Password = model.Password
        };
        _context.Users.Add(user);
        _context.SaveChanges();

        _context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = instructorRole.Id });
        _context.UserProfiles.Add(new UserProfile { UserId = user.Id, Phone = model.Phone });
        _context.SaveChanges();

        TempData["Message"] = "Đã thêm giảng viên mới.";
        return RedirectToAction(nameof(Instructors));
    }

    [HttpGet]
    public IActionResult EditInstructor(int id)
    {
        var user = _context.Users.Include(u => u.UserProfiles).FirstOrDefault(u => u.Id == id);
        if (user == null) return NotFound();

        var vm = new EditInstructorVm
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Phone = user.UserProfiles?.FirstOrDefault()?.Phone ?? ""
        };
        return View("Instructors/Edit", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditInstructor(EditInstructorVm model)
    {
        if (!ModelState.IsValid) return View("Instructors/Edit", model);
        var user = _context.Users.Include(u => u.UserProfiles).FirstOrDefault(u => u.Id == model.Id);
        if (user == null) return NotFound();

        user.FullName = model.FullName;
        user.Email = model.Email;
        var profile = user.UserProfiles?.FirstOrDefault();
        if (profile == null)
        {
            _context.UserProfiles.Add(new UserProfile { UserId = user.Id, Phone = model.Phone });
        }
        else
        {
            profile.Phone = model.Phone;
        }
        _context.SaveChanges();
        TempData["Message"] = "Cập nhật giảng viên thành công.";
        return RedirectToAction(nameof(Instructors));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteInstructor(int id)
    {
        var user = _context.Users
            .Include(u => u.UserRoles)
            .FirstOrDefault(u => u.Id == id);
        if (user == null) return NotFound();

        var courseLinks = _context.CourseInstructors.Where(ci => ci.UserId == id);
        _context.CourseInstructors.RemoveRange(courseLinks);
        _context.UserRoles.RemoveRange(user.UserRoles);
        var profile = _context.UserProfiles.FirstOrDefault(p => p.UserId == id);
        if (profile != null) _context.UserProfiles.Remove(profile);
        _context.Users.Remove(user);
        _context.SaveChanges();
        TempData["Message"] = "Đã xóa giảng viên.";
        return RedirectToAction(nameof(Instructors));
    }

    public IActionResult Lessons()
    {
        var lessons = _context.Lessons
            .Include(l => l.Course)
            .Select(l => new LessonRow
            {
                Id = l.Id,
                Title = l.Title,
                CourseName = l.Course != null ? l.Course.CourseName : "",
                OrderIndex = l.OrderIndex,
                DocumentUrl = l.DocumentUrl
            })
            .OrderBy(l => l.CourseName).ThenBy(l => l.OrderIndex)
            .ToList();
        return View("Lessons/Index", new LessonsPageVm { Lessons = lessons });
    }

    [HttpGet]
    public IActionResult CreateLesson()
    {
        ViewBag.Courses = _context.Courses.ToList();
        return View("Lessons/Create", new LessonFormVm());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateLesson(LessonFormVm model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Courses = _context.Courses.ToList();
            return View("Lessons/Create", model);
        }
        _context.Lessons.Add(new Lesson 
        { 
            Title = model.Title, 
            CourseId = model.CourseId,
            OrderIndex = model.OrderIndex,
            DocumentUrl = model.DocumentUrl,
            VideoUrl = model.VideoUrl,
            Content = model.Content,
            Duration = model.Duration,
            IsFreePreview = model.IsFreePreview,
            IsPublished = model.IsPublished
        });
        _context.SaveChanges();
        TempData["Message"] = "Đã tạo bài giảng.";
        return RedirectToAction(nameof(Lessons));
    }

    [HttpGet]
    public IActionResult EditLesson(int id)
    {
        var lesson = _context.Lessons.FirstOrDefault(l => l.Id == id);
        if (lesson == null) return NotFound();
        ViewBag.Courses = _context.Courses.ToList();
        return View("Lessons/Edit", new LessonFormVm 
        { 
            Id = lesson.Id, 
            Title = lesson.Title, 
            CourseId = lesson.CourseId,
            OrderIndex = lesson.OrderIndex,
            DocumentUrl = lesson.DocumentUrl,
            VideoUrl = lesson.VideoUrl,
            Content = lesson.Content,
            Duration = lesson.Duration,
            IsFreePreview = lesson.IsFreePreview,
            IsPublished = lesson.IsPublished
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditLesson(LessonFormVm model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Courses = _context.Courses.ToList();
            return View("Lessons/Edit", model);
        }
        var lesson = _context.Lessons.FirstOrDefault(l => l.Id == model.Id);
        if (lesson == null) return NotFound();
        lesson.Title = model.Title;
        lesson.CourseId = model.CourseId;
        lesson.OrderIndex = model.OrderIndex;
        lesson.DocumentUrl = model.DocumentUrl;
        lesson.VideoUrl = model.VideoUrl;
        lesson.Content = model.Content;
        lesson.Duration = model.Duration;
        lesson.IsFreePreview = model.IsFreePreview;
        lesson.IsPublished = model.IsPublished;
        _context.SaveChanges();
        TempData["Message"] = "Đã cập nhật bài giảng.";
        return RedirectToAction(nameof(Lessons));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteLesson(int id)
    {
        var lesson = _context.Lessons.FirstOrDefault(l => l.Id == id);
        if (lesson == null) return NotFound();
        _context.Lessons.Remove(lesson);
        _context.SaveChanges();
        TempData["Message"] = "Đã xóa bài giảng.";
        return RedirectToAction(nameof(Lessons));
    }

    public IActionResult Quiz()
    {
        var exams = _context.Exams
            .Include(e => e.Course)
            .ToList();
        return View("Quiz/Index", new QuizPageVm { Exams = exams });
    }

    [HttpGet]
    public IActionResult CreateQuiz()
    {
        ViewBag.Courses = _context.Courses.ToList();
        return View("Quiz/Create", new ExamFormVm());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateQuiz(ExamFormVm model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Courses = _context.Courses.ToList();
            return View("Quiz/Create", model);
        }
        var newExam = new Exam 
        { 
            Title = model.Title, 
            CourseId = model.CourseId,
            DurationMinutes = model.DurationMinutes,
            PassingScore = model.PassingScore,
            Description = model.Description,
            ShuffleQuestions = model.ShuffleQuestions,
            MaxAttempts = model.MaxAttempts,
            ShowAnswers = model.ShowAnswers
        };
        _context.Exams.Add(newExam);
        _context.SaveChanges();
        TempData["Message"] = "Đã tạo thông tin cơ bản của bài quiz. Xin mời thêm câu hỏi!";
        return RedirectToAction(nameof(EditQuiz), new { id = newExam.Id });
    }

    [HttpGet]
    public IActionResult EditQuiz(int id)
    {
        var exam = _context.Exams.FirstOrDefault(e => e.Id == id);
        if (exam == null) return NotFound();
        
        var questions = _context.Questions
            .Where(q => q.ExamId == id)
            .ToList();
            
        var questionIds = questions.Select(q => q.Id).ToList();
        var options = _context.QuestionOptions
            .Where(o => questionIds.Contains(o.QuestionId))
            .ToList();

        ViewBag.Questions = questions;
        ViewBag.Options = options;
        ViewBag.Courses = _context.Courses.ToList();
        
        return View("Quiz/Edit", new ExamFormVm 
        { 
            Id = exam.Id, 
            Title = exam.Title, 
            CourseId = exam.CourseId,
            DurationMinutes = exam.DurationMinutes,
            PassingScore = exam.PassingScore,
            Description = exam.Description,
            ShuffleQuestions = exam.ShuffleQuestions,
            MaxAttempts = exam.MaxAttempts,
            ShowAnswers = exam.ShowAnswers
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditQuiz(ExamFormVm model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Courses = _context.Courses.ToList();
            return View("Quiz/Edit", model);
        }
        var exam = _context.Exams.FirstOrDefault(e => e.Id == model.Id);
        if (exam == null) return NotFound();
        
        exam.Title = model.Title;
        exam.CourseId = model.CourseId;
        exam.DurationMinutes = model.DurationMinutes;
        exam.PassingScore = model.PassingScore;
        exam.Description = model.Description;
        exam.ShuffleQuestions = model.ShuffleQuestions;
        exam.MaxAttempts = model.MaxAttempts;
        exam.ShowAnswers = model.ShowAnswers;
        _context.SaveChanges();
        
        TempData["Message"] = "Đã cập nhật bài quiz.";
        return RedirectToAction(nameof(Quiz));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteQuiz(int id)
    {
        var exam = _context.Exams.FirstOrDefault(e => e.Id == id);
        if (exam == null) return NotFound();
        
        // cascade delete questions will be handled by EF if configured, otherwise might fail
        _context.Exams.Remove(exam);
        _context.SaveChanges();
        TempData["Message"] = "Đã xóa bài quiz.";
        return RedirectToAction(nameof(Quiz));
    }

    [HttpGet]
    public IActionResult QuizDetails(int id)
    {
        var exam = _context.Exams
            .Include(e => e.Course)
            .FirstOrDefault(e => e.Id == id);
            
        if (exam == null) return NotFound();

        var questions = _context.Questions
            .Where(q => q.ExamId == id)
            .ToList();
            
        var questionIds = questions.Select(q => q.Id).ToList();
        var options = _context.QuestionOptions
            .Where(o => questionIds.Contains(o.QuestionId))
            .ToList();

        ViewBag.Questions = questions;
        ViewBag.Options = options;
        
        return View("Quiz/Details", exam);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateQuestion(int examId, string content)
    {
        if (!string.IsNullOrWhiteSpace(content))
        {
            _context.Questions.Add(new Question { ExamId = examId, Content = content });
            _context.SaveChanges();
            TempData["Message"] = "Đã thêm câu hỏi.";
        }
        return RedirectToAction(nameof(EditQuiz), new { id = examId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditQuestion(int id, string content, int examId)
    {
        var question = _context.Questions.FirstOrDefault(q => q.Id == id);
        if (question != null && !string.IsNullOrWhiteSpace(content))
        {
            question.Content = content;
            _context.SaveChanges();
            TempData["Message"] = "Đã sửa câu hỏi.";
        }
        return RedirectToAction(nameof(EditQuiz), new { id = examId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteQuestion(int id, int examId)
    {
        var question = _context.Questions.FirstOrDefault(q => q.Id == id);
        if (question != null)
        {
            var options = _context.QuestionOptions.Where(o => o.QuestionId == id).ToList();
            _context.QuestionOptions.RemoveRange(options);
            _context.Questions.Remove(question);
            _context.SaveChanges();
            TempData["Message"] = "Đã xóa câu hỏi.";
        }
        return RedirectToAction(nameof(EditQuiz), new { id = examId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateOption(int questionId, int examId, string content, bool isCorrect = false)
    {
        if (!string.IsNullOrWhiteSpace(content))
        {
            _context.QuestionOptions.Add(new QuestionOption { QuestionId = questionId, Content = content, IsCorrect = isCorrect });
            _context.SaveChanges();
            TempData["Message"] = "Đã thêm đáp án.";
        }
        return RedirectToAction(nameof(EditQuiz), new { id = examId });
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditOption(int id, int examId, string content)
    {
        var option = _context.QuestionOptions.FirstOrDefault(o => o.Id == id);
        if (option != null && !string.IsNullOrWhiteSpace(content))
        {
            option.Content = content;
            _context.SaveChanges();
            TempData["Message"] = "Đã sửa đáp án.";
        }
        return RedirectToAction(nameof(EditQuiz), new { id = examId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteOption(int id, int examId)
    {
        var option = _context.QuestionOptions.FirstOrDefault(o => o.Id == id);
        if (option != null)
        {
            _context.QuestionOptions.Remove(option);
            _context.SaveChanges();
            TempData["Message"] = "Đã xóa đáp án.";
        }
        return RedirectToAction(nameof(EditQuiz), new { id = examId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult SetCorrectOption(int optionId, int questionId, int examId)
    {
        var options = _context.QuestionOptions.Where(o => o.QuestionId == questionId).ToList();
        foreach (var opt in options)
        {
            opt.IsCorrect = opt.Id == optionId;
        }
        _context.SaveChanges();
        TempData["Message"] = "Đã chọn đáp án đúng.";
        return RedirectToAction(nameof(EditQuiz), new { id = examId });
    }

    [HttpPost]
    public async Task<IActionResult> GenerateLessonContent([FromBody] AiRequestMsg req)
    {
        if (string.IsNullOrWhiteSpace(req.Topic)) return BadRequest();
        var content = await _aiService.GenerateLessonContentAsync(req.Topic);
        return Json(new { success = true, content });
    }

    [HttpPost]
    public async Task<IActionResult> GenerateExamQuestions([FromBody] AiExamRequestMsg req)
    {
        if (string.IsNullOrWhiteSpace(req.Topic) || req.ExamId == 0) return BadRequest();
        
        var questions = await _aiService.GenerateQuizQuestionsAsync(req.Topic, req.Count);
        foreach (var qDto in questions)
        {
            var dbQ = new Question { ExamId = req.ExamId, Content = qDto.Content };
            _context.Questions.Add(dbQ);
            _context.SaveChanges(); 
            
            foreach (var optDto in qDto.Options)
            {
                _context.QuestionOptions.Add(new QuestionOption 
                { 
                    QuestionId = dbQ.Id, 
                    Content = optDto.Content, 
                    IsCorrect = optDto.IsCorrect 
                });
            }
        }
        _context.SaveChanges();
        return Json(new { success = true });
    }

    public IActionResult Discounts() => View("Discounts/Index");

    public IActionResult Payments()
    {
        var courses = _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Level)
            .ToList();
        return View("Payments/Index", new PaymentPageVm { Courses = courses });
    }

    public IActionResult Revenue()
    {
        var learnerByCourse = _context.LearningProgress
            .GroupBy(lp => lp.CourseId)
            .Select(g => new { CourseId = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .ToList();

        var courseLookup = _context.Courses.ToDictionary(c => c.Id, c => c.CourseName);
        const decimal MOCK_PRICE = 500000m; // 500,000 VND per course enroll

        var top = learnerByCourse
            .Take(10)
            .Select(g => new RevenueCourseVm
            {
                CourseName = courseLookup.TryGetValue(g.CourseId, out var name) ? name : $"Course {g.CourseId}",
                Learners = g.Count,
                Revenue = g.Count * MOCK_PRICE
            })
            .OrderByDescending(x => x.Revenue)
            .ToList();

        int totalLearners = learnerByCourse.Sum(x => x.Count);

        var monthlyRev = new List<decimal> { 12000000, 18000000, 14000000, 25000000, 32000000, 28000000 };
        monthlyRev.Add(totalLearners * MOCK_PRICE / 3); // Hiện tại giả lập

        var vm = new RevenuePageVm
        {
            TotalLearners = totalLearners,
            TotalCourses = courseLookup.Count,
            TotalRevenue = totalLearners * MOCK_PRICE,
            TopCourses = top,
            MonthlyRevenue = monthlyRev
        };
        
        return View(vm);
    }

    [HttpGet]
    public IActionResult CreateAccount()
    {
        ViewBag.Roles = _context.Roles.ToList();
        ViewBag.Categories = _context.CourseCategories.ToList();
        return View("Accounts/Create");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateAccount(RegisterViewModel model, int roleId, int? categoryId)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Roles = _context.Roles.ToList();
            ViewBag.Categories = _context.CourseCategories.ToList();
            return View("Accounts/Create", model);
        }

        var user = new User
        {
            Username = model.Username,
            FullName = model.FullName,
            Email = model.Email,
            Password = model.Password
        };
        _context.Users.Add(user);
        _context.SaveChanges();

        _context.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roleId });
        _context.UserProfiles.Add(new UserProfile { UserId = user.Id, Phone = model.Phone });

        var role = _context.Roles.FirstOrDefault(r => r.Id == roleId);
        if (role != null && role.Name == "Student" && categoryId.HasValue)
        {
            var category = _context.CourseCategories.FirstOrDefault(c => c.Id == categoryId.Value);
            var prefix = category != null ? GetPrefix(category.Name) : "STU";
            user.StudentCode = $"{prefix}{user.Id:D4}";
        }

        _context.SaveChanges();
        TempData["Message"] = "Đã tạo tài khoản mới";
        return RedirectToAction(nameof(Accounts));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AssignInstructorToCourse(int courseId, int instructorId)
    {
        var exists = _context.CourseInstructors.Any(ci => ci.CourseId == courseId && ci.UserId == instructorId);
        if (!exists)
        {
            _context.CourseInstructors.Add(new CourseInstructor { CourseId = courseId, UserId = instructorId });
            _context.SaveChanges();
            TempData["Message"] = "Đã gán giảng viên cho khóa học.";
        }
        return RedirectToAction(nameof(Instructors));
    }
}
