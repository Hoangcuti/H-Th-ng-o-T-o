using COTHUYPRO.Models;
using COTHUYPRO.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
            PendingCertificates = _context.UserCertificates.Count(),
            SoldCourseCount = _context.ClassStudents.Count(),
            TotalRevenue = _context.ClassStudents.Include(cs => cs.Class).ThenInclude(c => c!.Course).Sum(cs => cs.Class != null && cs.Class.Course != null ? cs.Class.Course.Price : 0),
            AvgStudentProgress = _context.LearningProgresses.Any() ? _context.LearningProgresses.Average(lp => lp.Status == "Completed" ? 100.0 : 0.0) : 0
        };

        var latestUsers = _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .OrderByDescending(u => u.CreatedAt)
            .Take(8)
            .ToList();

        var learnerByCourse = _context.ClassStudents
            .Include(cs => cs.Class)
            .GroupBy(cs => cs.Class!.CourseId)
            .Select(g => new { CourseId = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .Take(5)
            .ToList();

        var topCourses = learnerByCourse
            .Select(g =>
            {
                var c = _context.Courses.FirstOrDefault(x => x.Id == g.CourseId);
                return new RevenueCourseVm
                {
                    CourseName = c?.CourseName ?? $"Course {g.CourseId}",
                    Learners = g.Count,
                    Revenue = g.Count * (c?.Price ?? 0)
                };
            })
            .ToList();

        var latestCourses = _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Level)
            .OrderByDescending(c => c.Id)
            .Take(6)
            .ToList();

        // 1. Line Chart Data: Revenue by Month (Last 12 Months)
        var revenuePoints = _context.ClassStudents
            .Include(cs => cs.Class).ThenInclude(c => c!.Course)
            .Where(cs => cs.CreatedAt >= DateTime.Now.AddMonths(-12))
            .ToList()
            .GroupBy(cs => new { cs.CreatedAt.Year, cs.CreatedAt.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new
            {
                Label = $"T{g.Key.Month}/{g.Key.Year % 100}",
                Value = g.Sum(cs => cs.Class != null && cs.Class.Course != null ? cs.Class.Course.Price : 0)
            })
            .ToList();

        // 2. Pie Chart Data: Course Distribution (Show top 8 courses with enrollment)
        var courseDataQuery = _context.Courses
            .Select(c => new
            {
                Label = c.CourseName,
                Value = _context.ClassStudents.Count(cs => cs.Class != null && cs.Class.CourseId == c.Id)
            })
            .OrderByDescending(x => x.Value)
            .Take(8)
            .ToList();

        var vm = new AdminIndexViewModel
        {
            Stats = stats,
            RecentUsers = latestUsers,
            RecentCourses = latestCourses,
            TopCourses = topCourses,
            RevenueLabels = revenuePoints.Select(x => x.Label).ToArray(),
            RevenueData = revenuePoints.Select(x => x.Value).ToArray(),
            CourseLabels = courseDataQuery.Select(x => x.Label).ToArray(),
            CourseData = courseDataQuery.Select(x => x.Value).ToArray()
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
            .ToList()
            .Select(c => new AdminCourseRow
            {
                Course = c,
                CategoryName = c.Category != null ? c.Category.Name : "Chua phan loai",
                LevelName = c.Level != null ? c.Level.Name : "Khong cap do",
                ClassCount = c.Classes.Count,
                LearnerCount = _context.ClassStudents.Count(cs => cs.Class != null && cs.Class.CourseId == c.Id),
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
            LevelId = model.LevelId,
            Price = model.Price,
            ImageUrl = model.ImageUrl
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
            LevelId = course.LevelId,
            Price = course.Price,
            ImageUrl = course.ImageUrl
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
        course.Price = model.Price;
        course.ImageUrl = model.ImageUrl;
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
            .Include(u => u.ClassStudents).ThenInclude(cs => cs.Class).ThenInclude(c => c!.Course)
            .Where(u => u.UserRoles.Any(r => r.Role!.Name == "Student"))
            .ToList()
            .Select(u => new StudentVm
            {
                User = u,
                Department = u.Department != null ? u.Department.Name : "-",
                Position = u.Position != null ? u.Position.Name : "-",
                CompletedCourses = 0, // Tạm thời để 0, sẽ tính sau nếu cần
                StudentCode = !string.IsNullOrEmpty(u.StudentCode) ? u.StudentCode : "Chưa cấp",
                EnrolledCourses = u.ClassStudents.Where(cs => cs.Class != null && cs.Class.Course != null).Select(cs => cs.Class!.Course!.CourseName).Distinct().ToList()
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
            .ToList()
            .Select(u => new InstructorVm
            {
                User = u,
                CourseCount = u.TeachingCourses.Count,
                TaughtCourses = u.TeachingCourses.Select(tc => tc.Course!).ToList()
            })
            .ToList();

        var applications = _context.InstructorCourseApplications
            .Include(a => a.User)
            .Include(a => a.Course)
            .OrderByDescending(a => a.ApplyDate)
            .ToList();

        var courses = _context.Courses.ToList();
        ViewBag.Applications = applications;
        return View("Instructors/Index", new InstructorsPageVm { Instructors = instructors, Courses = courses });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RemoveInstructorFromCourse(int courseId, int instructorId)
    {
        var ci = _context.CourseInstructors.FirstOrDefault(x => x.CourseId == courseId && x.UserId == instructorId);
        if (ci != null)
        {
            _context.CourseInstructors.Remove(ci);
            _context.SaveChanges();
            TempData["Message"] = "Đã gỡ giảng viên khỏi khóa học.";
        }
        return RedirectToAction(nameof(InstructorCourses), new { id = instructorId });
    }

    public IActionResult StudentDetails(int id)
    {
        var user = _context.Users
            .Include(u => u.ClassStudents).ThenInclude(cs => cs.Class).ThenInclude(c => c!.Course)
            .FirstOrDefault(u => u.Id == id);

        if (user == null) return NotFound();

        var vm = new StudentDetailsVm { User = user };

        // 1. Calculate Course Progress
        var enrolledCourseIds = user.ClassStudents
            .Where(cs => cs.Class != null && cs.Class.CourseId != 0)
            .Select(cs => cs.Class!.CourseId)
            .Distinct()
            .ToList();

        foreach (var courseIdNullable in enrolledCourseIds)
        {
            int courseId = courseIdNullable.GetValueOrDefault();
            var course = _context.Courses.Find(courseId);
            if (course == null) continue;

            var totalLessons = _context.Lessons.Count(l => l.CourseId == courseId);
            var completedLessons = _context.LearningProgresses
                .Count(lp => lp.UserId == id && lp.Lesson!.CourseId == courseId && lp.Status == "Completed");

            vm.CourseProgresses.Add(new CourseProgressVm
            {
                CourseId = courseId,
                CourseName = course.CourseName,
                TotalLessons = totalLessons,
                CompletedLessons = completedLessons
            });
        }

        // 2. Fetch Quiz Results
        vm.QuizResults = _context.ExamResults
            .Include(r => r.Attempt).ThenInclude(a => a!.Exam)
            .Where(r => r.Attempt!.UserId == id)
            .OrderByDescending(r => r.Attempt!.StartedAt)
            .Select(r => new StudentQuizResultVm
            {
                ExamTitle = r.Attempt!.Exam!.Title,
                Score = r.Score,
                AttemptDate = r.Attempt.StartedAt,
                IsPassed = r.Score >= (r.Attempt.Exam.PassingScore / 10.0)
            })
            .ToList();

        return View("Students/Details", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult HandleApplication(int id, string action)
    {
        var app = _context.InstructorCourseApplications.FirstOrDefault(a => a.Id == id);
        if (app == null) return NotFound();

        if (action == "Approve")
        {
            app.Status = "Approved";
            // Gán giảng viên vào khóa học luôn
            var exists = _context.CourseInstructors.Any(ci => ci.CourseId == app.CourseId && ci.UserId == app.UserId);
            if (!exists)
            {
                _context.CourseInstructors.Add(new CourseInstructor { CourseId = app.CourseId, UserId = app.UserId });
            }
            TempData["Message"] = "Đã phê duyệt đơn đăng ký giảng dạy.";
        }
        else if (action == "Reject")
        {
            app.Status = "Rejected";
            TempData["Message"] = "Đã từ chối đơn đăng ký giảng dạy.";
        }

        _context.SaveChanges();
        return RedirectToAction(nameof(Instructors));
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
        var courses = _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Level)
            .Select(c => new AdminCourseRow
            {
                Course = c,
                CategoryName = c.Category != null ? c.Category.Name : "N/A",
                LevelName = c.Level != null ? c.Level.Name : "N/A",
                ClassCount = _context.Lessons.Count(l => l.CourseId == c.Id), // TRẢ VỀ SỐ BÀI HỌC (LỘ TRÌNH)
                LearnerCount = _context.ClassStudents.Count(cs => cs.Class != null && cs.Class.CourseId == c.Id)
            })
            .ToList();
        return View("Lessons/Index", new CoursesPageVm { Courses = courses });
    }

    public IActionResult CourseLessons(int courseId)
    {
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
                    .Where(l => l.ChapterId == c.Id && l.Status == "Approved")
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

        var pendingLessons = _context.Lessons
            .Where(l => l.CourseId == courseId && l.Status == "Pending")
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

        // Also get lessons NOT in any chapter (Approved only)
        var orphanLessons = _context.Lessons
            .Where(l => l.CourseId == courseId && l.ChapterId == null && l.Status == "Approved")
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
            Chapters = chapters,
            PendingLessons = pendingLessons
        });
    }

    [HttpGet]
    public IActionResult CreateBulk(int courseId)
    {
        var course = _context.Courses.FirstOrDefault(c => c.Id == courseId);
        if (course == null) return NotFound();

        var vm = new CreateBulkVm
        {
            CourseId = courseId,
            AvailableCourses = _context.Courses.OrderBy(c => c.CourseName).ToList()
        };
        return View("Lessons/CreateBulk", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateBulk(CreateBulkVm model)
    {
        if (string.IsNullOrWhiteSpace(model.ChapterTitle))
        {
            ModelState.AddModelError("", "Vui lòng nhập tên Nội dung chương trình.");
            model.AvailableCourses = _context.Courses.OrderBy(c => c.CourseName).ToList();
            return View("Lessons/CreateBulk", model);
        }

        // 1. Create Chapter
        var chapter = new CourseChapter
        {
            CourseId = model.CourseId,
            Title = model.ChapterTitle,
            OrderIndex = _context.CourseChapters.Count(c => c.CourseId == model.CourseId) + 1
        };
        _context.CourseChapters.Add(chapter);
        _context.SaveChanges();

        // 2. Create Lessons & Quizzes
        foreach (var entry in model.Lessons.Where(e => !string.IsNullOrWhiteSpace(e.Title)))
        {
            var lesson = new Lesson
            {
                CourseId = model.CourseId, // Use the shared course ID from the form
                ChapterId = chapter.Id,
                Title = entry.Title,
                VideoUrl = entry.VideoUrl,
                OrderIndex = entry.OrderIndex > 0 ? entry.OrderIndex : (_context.Lessons.Count(l => l.CourseId == model.CourseId) + 1),
                Duration = entry.Duration,
                DocumentUrl = entry.DocumentUrl,
                Content = entry.ContentHtml,
                IsFreePreview = entry.IsFreePreview,
                IsPublished = entry.IsPublished,
                Status = "Pending"
            };
            _context.Lessons.Add(lesson);
            _context.SaveChanges();

            if (!string.IsNullOrWhiteSpace(entry.QuizTitle))
            {
                var exam = new Exam
                {
                    CourseId = model.CourseId,
                    LessonId = lesson.Id,
                    Title = entry.QuizTitle,
                    PassingScore = 10,
                    DurationMinutes = 15,
                    MaxAttempts = 99
                };
                _context.Exams.Add(exam);
                _context.SaveChanges();
            }
        }

        TempData["Message"] = $"Đã tạo thành công chương '{model.ChapterTitle}' và các bài học liên quan.";
        return RedirectToAction(nameof(CourseLessons), new { courseId = model.CourseId });
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
        string? docUrl = model.DocumentUrl;
        if (model.DocumentFile != null && model.DocumentFile.Length > 0)
        {
            var fileName = Guid.NewGuid() + Path.GetExtension(model.DocumentFile.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "lessons", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            using (var stream = new FileStream(filePath, FileMode.Create))
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
            Status = "Approved",
            CreatedByUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)
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
        if (model.DocumentFile != null && model.DocumentFile.Length > 0)
        {
            var fileName = Guid.NewGuid() + Path.GetExtension(model.DocumentFile.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "lessons", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                model.DocumentFile.CopyTo(stream);
            }
            lesson.DocumentUrl = "/uploads/lessons/" + fileName;
        }
        else
        {
            lesson.DocumentUrl = model.DocumentUrl;
        }

        lesson.Title = model.Title;
        lesson.CourseId = model.CourseId;
        lesson.OrderIndex = model.OrderIndex;
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

        // 1. Delete LearningProgresses
        var progresses = _context.LearningProgresses.Where(lp => lp.LessonId == id).ToList();
        _context.LearningProgresses.RemoveRange(progresses);

        // 2. Delete Exams and related attempts/results
        var exams = _context.Exams.Where(e => e.LessonId == id).ToList();
        foreach (var exam in exams)
        {
            var attempts = _context.ExamAttempts.Where(a => a.ExamId == exam.Id).ToList();
            foreach (var attempt in attempts)
            {
                var results = _context.ExamResults.Where(r => r.AttemptId == attempt.Id).ToList();
                _context.ExamResults.RemoveRange(results);
            }
            _context.ExamAttempts.RemoveRange(attempts);
        }
        _context.Exams.RemoveRange(exams);

        // 3. Finally delete the lesson
        _context.Lessons.Remove(lesson);
        _context.SaveChanges();

        TempData["Message"] = "Đã xóa LỘ TRÌNH BÀI HỌC và các dữ liệu liên quan.";
        return RedirectToAction("CourseLessons", new { courseId = lesson.CourseId });
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
        ViewBag.Lessons = _context.Lessons.ToList(); // For simple dropdown initial load
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
            LessonId = model.LessonId,
            DurationMinutes = model.DurationMinutes,
            PassingScore = model.PassingScore,
            Description = model.Description,
            ShuffleQuestions = model.ShuffleQuestions,
            MaxAttempts = model.MaxAttempts,
            ShowAnswers = model.ShowAnswers,
            ExamType = model.ExamType,
            Status = "Approved"
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
        ViewBag.Lessons = _context.Lessons.Where(l => l.CourseId == exam.CourseId).ToList();

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
            ShowAnswers = exam.ShowAnswers,
            ExamType = exam.ExamType
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
        exam.LessonId = model.LessonId;
        exam.DurationMinutes = model.DurationMinutes;
        exam.PassingScore = model.PassingScore;
        exam.Description = model.Description;
        exam.ShuffleQuestions = model.ShuffleQuestions;
        exam.MaxAttempts = model.MaxAttempts;
        exam.ShowAnswers = model.ShowAnswers;
        exam.ExamType = model.ExamType;
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

    [HttpPost]
    public async Task<IActionResult> GenerateAiContent([FromBody] AiRequest model)
    {
        if (string.IsNullOrWhiteSpace(model.Topic)) return BadRequest("Topic is required");
        var content = await _aiService.GenerateLessonContentAsync(model.Topic);
        return Json(new { content });
    }
    public IActionResult Revenue()
    {
        var purchases = _context.ClassStudents
            .Include(cs => cs.User)
            .Include(cs => cs.Class).ThenInclude(c => c!.Course)
            .Where(cs => cs.IsPaid)
            .OrderByDescending(cs => cs.PaymentDate ?? cs.CreatedAt)
            .ToList();

        var totalRevenue = purchases.Sum(p => p.PaidAmount);
        var thisMonth = DateTime.Now.Month;
        var thisYear = DateTime.Now.Year;
        var monthlyRevenue = purchases
            .Where(p => (p.PaymentDate ?? p.CreatedAt).Month == thisMonth && (p.PaymentDate ?? p.CreatedAt).Year == thisYear)
            .Sum(p => p.PaidAmount);

        // Chart 1: Revenue by month (last 6 months)
        var last6Months = Enumerable.Range(0, 6)
            .Select(i => DateTime.Now.AddMonths(-i))
            .OrderBy(d => d)
            .ToList();

        var monthlyLabels = last6Months.Select(d => d.ToString("MM/yyyy")).ToArray();
        var monthlyData = last6Months.Select(d => purchases
            .Where(p => (p.PaymentDate ?? p.CreatedAt).Month == d.Month && (p.PaymentDate ?? p.CreatedAt).Year == d.Year)
            .Sum(p => p.PaidAmount))
            .ToArray();

        // Chart 2: Revenue by course
        var courseGroups = purchases
            .GroupBy(p => p.Class?.Course?.CourseName ?? "Ẩn danh")
            .Select(g => new { Name = g.Key, Total = g.Sum(p => p.PaidAmount) })
            .OrderByDescending(x => x.Total)
            .Take(5)
            .ToList();

        var vm = new RevenueDashboardVm
        {
            TotalRevenue = totalRevenue,
            MonthlyRevenue = monthlyRevenue,
            NewEnrollmentsCount = purchases.Count(p => (p.PaymentDate ?? p.CreatedAt) >= DateTime.Now.AddDays(-30)),
            RecentPurchases = purchases.Take(10).Select(p => new EnrollmentRow
            {
                StudentName = p.User?.FullName ?? "N/A",
                CourseName = p.Class?.Course?.CourseName ?? "N/A",
                Amount = p.PaidAmount,
                Date = p.PaymentDate ?? p.CreatedAt
            }).ToList(),
            MonthlyLabels = monthlyLabels,
            MonthlyData = monthlyData,
            CourseLabels = courseGroups.Select(x => x.Name).ToArray(),
            CourseData = courseGroups.Select(x => x.Total).ToArray()
        };

        return View("Revenue/Index", vm);
    }

    [HttpGet]
    public IActionResult MigrateStatus()
    {
        try
        {
            _context.Database.ExecuteSqlRaw("IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Lessons') AND name = 'Status') ALTER TABLE Lessons ADD Status NVARCHAR(20) DEFAULT 'Approved' NOT NULL;");
            return Content("Migration successful! Status column added.");
        }
        catch (Exception ex)
        {
            return Content("Migration failed: " + ex.Message);
        }
    }

    [HttpPost]
    public IActionResult ApproveLesson(int id)
    {
        var lesson = _context.Lessons.FirstOrDefault(l => l.Id == id);
        if (lesson == null) return NotFound();
        lesson.Status = "Approved";
        _context.SaveChanges();
        return Json(new { success = true });
    }

    [HttpPost]
    public IActionResult RejectLesson(int id)
    {
        var lesson = _context.Lessons.FirstOrDefault(l => l.Id == id);
        if (lesson == null) return NotFound();
        lesson.Status = "Draft";
        _context.SaveChanges();
        return Json(new { success = true });
    }

    [HttpPost]
    public IActionResult SubmitLesson(int id)
    {
        var lesson = _context.Lessons.FirstOrDefault(l => l.Id == id);
        if (lesson == null) return NotFound();
        lesson.Status = "Pending";
        _context.SaveChanges();
        return Json(new { success = true });
    }

    // ==========================================
    // PHẦN 4: PHÊ DUYỆT NỘI DUNG & LỊCH SỬ
    // ==========================================

    public IActionResult PendingContent()
    {
        var pendingLessons = _context.Lessons
            .Include(l => l.Course)
            .Include(l => l.CreatedByUser)
            .Include(l => l.UpdatedByUser)
            .Where(l => l.Status == "Pending")
            .OrderByDescending(l => l.UpdatedAt ?? l.CreatedAt)
            .ToList();

        var pendingExams = _context.Exams
            .Include(e => e.Course)
            .Include(e => e.CreatedByUser)
            .Where(e => e.Status == "Pending")
            .OrderByDescending(e => e.UpdatedAt ?? e.CreatedAt)
            .ToList();

        var pendingAssignments = _context.Assignments
            .Include(a => a.Course)
            .Include(a => a.CreatedByUser)
            .Where(a => a.Status == "Pending")
            .OrderByDescending(a => a.UpdatedAt ?? a.CreatedAt)
            .ToList();

        var pendingApplications = _context.InstructorCourseApplications
            .Include(a => a.User)
            .Include(a => a.Course)
            .Where(a => a.Status == "Pending")
            .OrderByDescending(a => a.ApplyDate)
            .ToList();

        ViewBag.PendingLessons = pendingLessons;
        ViewBag.PendingExams = pendingExams;
        ViewBag.PendingAssignments = pendingAssignments;
        ViewBag.PendingApplications = pendingApplications;
        return View();
    }

    [HttpPost]
    public IActionResult ApproveAssignment(int id)
    {
        var item = _context.Assignments.FirstOrDefault(a => a.Id == id);
        if (item == null) return NotFound();
        item.Status = "Approved";
        _context.SaveChanges();
        return Json(new { success = true });
    }

    [HttpPost]
    public IActionResult RejectAssignment(int id)
    {
        var item = _context.Assignments.FirstOrDefault(a => a.Id == id);
        if (item == null) return NotFound();
        item.Status = "Draft";
        _context.SaveChanges();
        return Json(new { success = true });
    }

    [HttpPost]
    public IActionResult ApproveExam(int id)
    {
        var exam = _context.Exams.FirstOrDefault(e => e.Id == id);
        if (exam == null) return NotFound();
        exam.Status = "Approved";
        _context.SaveChanges();
        return Json(new { success = true });
    }

    [HttpPost]
    public IActionResult RejectExam(int id)
    {
        var exam = _context.Exams.FirstOrDefault(e => e.Id == id);
        if (exam == null) return NotFound();
        exam.Status = "Draft";
        _context.SaveChanges();
        return Json(new { success = true });
    }

    public IActionResult ContentHistory()
    {
        var lessons = _context.Lessons
            .Include(l => l.Course)
            .Include(l => l.CreatedByUser)
            .Include(l => l.UpdatedByUser)
            .OrderByDescending(l => l.UpdatedAt ?? l.CreatedAt)
            .Take(100)
            .ToList();

        var exams = _context.Exams
            .Include(e => e.Course)
            .Include(e => e.CreatedByUser)
            .Include(e => e.UpdatedByUser)
            .OrderByDescending(e => e.UpdatedAt ?? e.CreatedAt)
            .Take(100)
            .ToList();

        var assignments = _context.Assignments
            .Include(a => a.Course)
            .Include(a => a.CreatedByUser)
            .Include(a => a.UpdatedByUser)
            .OrderByDescending(a => a.UpdatedAt ?? a.CreatedAt)
            .Take(100)
            .ToList();

        ViewBag.Lessons = lessons;
        ViewBag.Exams = exams;
        ViewBag.Assignments = assignments;
        return View();
    }

    // ==========================================
    // GỠ GIẢNG VIÊN KHỎI KHÓA HỌC (không xóa tài khoản)
    // ==========================================

    public IActionResult InstructorCourses(int id)
    {
        var instructor = _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefault(u => u.Id == id);
        if (instructor == null) return NotFound();

        var courses = _context.CourseInstructors
            .Include(ci => ci.Course)
            .Where(ci => ci.UserId == id)
            .ToList();

        ViewBag.Instructor = instructor;
        ViewBag.Courses = courses;
        return View();
    }

    // ==========================================
    // QUẢN LÝ NĂM - KỲ - BLOCK
    // ==========================================

    public IActionResult AcademicYears()
    {
        var years = _context.AcademicYears.OrderByDescending(y => y.YearNumber).ToList();
        return View("AcademicYears/Index", years);
    }

    [HttpPost]
    public IActionResult CreateAcademicYear(int yearNumber)
    {
        if (yearNumber > 2000)
        {
            _context.AcademicYears.Add(new AcademicYear { YearNumber = yearNumber });
            _context.SaveChanges();
            TempData["Message"] = "Đã thêm năm học mới.";
        }
        return RedirectToAction(nameof(AcademicYears));
    }

    public IActionResult Semesters()
    {
        var semesters = _context.Semesters
            .Include(s => s.Year)
            .OrderByDescending(s => s.Year!.YearNumber)
            .ThenBy(s => s.SemesterName)
            .ToList();
        ViewBag.Years = _context.AcademicYears.ToList();
        return View("Semesters/Index", semesters);
    }

    [HttpPost]
    public IActionResult CreateSemester(string name, int yearId)
    {
        if (!string.IsNullOrEmpty(name))
        {
            _context.Semesters.Add(new Semester { SemesterName = name, YearId = yearId });
            _context.SaveChanges();
            TempData["Message"] = "Đã thêm kỳ học mới.";
        }
        return RedirectToAction(nameof(Semesters));
    }

    public IActionResult Blocks()
    {
        var blocks = _context.Blocks
            .Include(b => b.Semester).ThenInclude(s => s!.Year)
            .OrderByDescending(b => b.Semester!.Year!.YearNumber)
            .ThenBy(b => b.Semester!.SemesterName)
            .ThenBy(b => b.BlockName)
            .ToList();
        ViewBag.Semesters = _context.Semesters.Include(s => s.Year).ToList();
        return View("Blocks/Index", blocks);
    }

    [HttpPost]
    public IActionResult CreateBlock(string name, int semesterId)
    {
        if (!string.IsNullOrEmpty(name))
        {
            _context.Blocks.Add(new Block { BlockName = name, SemesterId = semesterId });
            _context.SaveChanges();
            TempData["Message"] = "Đã thêm Block mới.";
        }
        return RedirectToAction(nameof(Blocks));
    }

    // ==========================================
    // CẬP NHẬT QUẢN LÝ LỚP HỌC (CLASS CODE)
    // ==========================================

    public IActionResult Classes()
    {
        var classes = _context.Classes
            .Include(c => c.Course)
            .Include(c => c.Instructor)
            .Include(c => c.Block).ThenInclude(b => b!.Semester).ThenInclude(s => s!.Year)
            .ToList();

        ViewBag.Courses = _context.Courses.ToList();
        var instructorRole = _context.Roles.FirstOrDefault(r => r.Name == "Instructor");
        ViewBag.Instructors = _context.Users
            .Where(u => u.UserRoles.Any(ur => ur.RoleId == instructorRole!.Id))
            .ToList();
        ViewBag.Blocks = _context.Blocks.Include(b => b.Semester).ThenInclude(s => s!.Year).ToList();

        return View("Classes/Index", classes);
    }

    [HttpPost]
    public IActionResult CreateClass(string classCode, int? courseId, int? instructorId, int? blockId)
    {
        if (string.IsNullOrEmpty(classCode))
        {
            TempData["Error"] = "Vui lòng nhập Mã lớp.";
            return RedirectToAction(nameof(Classes));
        }

        if (_context.Classes.Any(c => c.ClassCode == classCode.Trim()))
        {
            TempData["Error"] = "This class code already exists. Please choose a different code.";
            return RedirectToAction(nameof(Classes));
        }

        var newClass = new Class
        {
            ClassCode = classCode.Trim(),
            CourseId = courseId,
            InstructorId = instructorId,
            BlockId = blockId
        };
        _context.Classes.Add(newClass);
        _context.SaveChanges();

        TempData["Message"] = $"Created class {classCode} successfully.";
        return RedirectToAction(nameof(Classes));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AssignInstructorToClass(int classId, int instructorId)
    {
        var cls = _context.Classes.Find(classId);
        if (cls == null) return NotFound();

        cls.InstructorId = instructorId;
        _context.SaveChanges();
        TempData["Message"] = "Instructor assigned to class successfully.";
        return RedirectToAction(nameof(Classes));
    }

    public IActionResult ClassDetails(int classId)
    {
        var cls = _context.Classes
            .Include(c => c.Course)
            .Include(c => c.Instructor)
            .Include(c => c.Block).ThenInclude(b => b!.Semester).ThenInclude(s => s!.Year)
            .Include(c => c.ClassStudents).ThenInclude(cs => cs.User)
            .FirstOrDefault(c => c.Id == classId);

        if (cls == null) return NotFound();

        var detail = new ClassDetailVm
        {
            ClassInfo = cls,
            Lessons = _context.Lessons.Where(l => l.CourseId == cls.CourseId).OrderBy(l => l.OrderIndex).ToList(),
            Exams = _context.Exams.Where(e => e.CourseId == cls.CourseId).OrderByDescending(e => e.CreatedAt).ToList(),
            Assignments = _context.Assignments.Where(a => a.CourseId == cls.CourseId).OrderByDescending(a => a.CreatedAt).ToList()
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
                .Include(r => r.Attempt).ThenInclude(a => a.Exam)
                .Where(r => r.Attempt != null && r.Attempt.UserId == student.UserId && r.Attempt.Exam != null && r.Attempt.Exam.CourseId == cls.CourseId)
                .OrderByDescending(r => r.Attempt.StartedAt)
                .Select(r => new ClassExamResultVm
                {
                    ExamId = r.Attempt.ExamId,
                    ExamTitle = r.Attempt.Exam.Title,
                    Score = r.Score,
                    TotalQuestions = r.TotalQuestions,
                    AttemptDate = r.Attempt.StartedAt,
                    IsPassed = r.Score >= (r.Attempt.Exam.PassingScore / 10.0)
                })
                .ToList();

            studentHistory.AssignmentSubmissions = _context.AssignmentSubmissions
                .Include(s => s.Assignment)
                .Where(s => s.StudentId == student.UserId && s.Assignment != null && s.Assignment.CourseId == cls.CourseId)
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

            detail.StudentHistories.Add(studentHistory);
        }

        return View("Classes/Details", detail);
    }
}

public class AiRequest
{
    public string Topic { get; set; } = string.Empty;
}

public class RevenueDashboardVm
{
    public decimal TotalRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public int NewEnrollmentsCount { get; set; }
    public List<EnrollmentRow> RecentPurchases { get; set; } = new();
    public string[] MonthlyLabels { get; set; } = Array.Empty<string>();
    public decimal[] MonthlyData { get; set; } = Array.Empty<decimal>();
    public string[] CourseLabels { get; set; } = Array.Empty<string>();
    public decimal[] CourseData { get; set; } = Array.Empty<decimal>();
}

public class EnrollmentRow
{
    public string StudentName { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
}
