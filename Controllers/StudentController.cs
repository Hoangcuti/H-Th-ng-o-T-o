using COTHUYPRO.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace COTHUYPRO.Controllers;

[Authorize(Policy = "StudentOnly")]
public class StudentController : Controller
{
    private readonly TrainingContext _context;

    public StudentController(TrainingContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");
        var userId = int.Parse(userIdString);
        
        var enrollmentsQuery = _context.ClassStudents
            .Include(cs => cs.Class)!.ThenInclude(c => c!.Course)!.ThenInclude(c => c!.Category)
            .Include(cs => cs.Class)!.ThenInclude(c => c!.Course)!.ThenInclude(c => c!.Level)
            .AsQueryable();

        if (User.IsInRole("Admin"))
        {
            // Admin sees all courses in their "My Courses" area for testing
            var allCourses = _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Level)
                .ToList();
            
            var adminCourses = allCourses.Select(c => new {
                Enrollment = new ClassStudent { Class = new Class { Course = c, CourseId = c.Id }, IsPaid = true },
                Course = c,
                TotalLessonsCount = _context.Lessons.Count(l => l.CourseId == c.Id),
                Progress = 0 // Admins start at 0 but can access all
            }).ToList();
            
            ViewBag.MyCourses = adminCourses;
            return View();
        }

        var enrollments = enrollmentsQuery
            .Where(cs => cs.UserId == userId && cs.IsPaid)
            .ToList();

        // Map to a more detailed ViewModel list
            var myCourses = enrollments.Select(e => {
                var courseId = e.Class!.CourseId;
                var allLessons = _context.Lessons.Where(l => l.CourseId == courseId).ToList();
                double totalProgressScore = 0;
                
                if (allLessons.Any())
                {
                    foreach (var lesson in allLessons)
                    {
                        var hasExam = _context.Exams.Any(ex => ex.LessonId == lesson.Id);
                        if (!hasExam)
                        {
                            var lp = _context.LearningProgresses.FirstOrDefault(p => p.UserId == userId && p.LessonId == lesson.Id && p.Status == "Completed");
                            if (lp != null) totalProgressScore += 1.0;
                        }
                        else
                        {
                            // Get best quiz result for this lesson
                            var bestResult = _context.ExamResults
                                .Include(r => r.Attempt)
                                .Where(r => r.Attempt!.UserId == userId && r.Attempt.Exam!.LessonId == lesson.Id)
                                .OrderByDescending(r => (double)r.CorrectCount / r.TotalQuestions)
                                .FirstOrDefault();
                                
                            if (bestResult != null && bestResult.TotalQuestions > 0)
                            {
                                totalProgressScore += (double)bestResult.CorrectCount / bestResult.TotalQuestions;
                            }
                        }
                    }
                }

                return new {
                    Enrollment = e,
                    Course = e.Class.Course,
                    TotalLessonsCount = allLessons.Count,
                    Progress = allLessons.Count > 0 ? (int)(totalProgressScore * 100 / allLessons.Count) : 0
                };
            }).ToList();

        ViewBag.MyCourses = myCourses;
        return View();
    }

    public IActionResult CourseDetail(int id)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var course = _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Level)
            .Include(c => c.Instructors).ThenInclude(i => i.User)
            .FirstOrDefault(c => c.Id == id);

        if (course == null) return NotFound();
        
        // CHECK PAYMENT STATUS (Admin bypass)
        bool isAdmin = User.IsInRole("Admin");
        var enrollment = _context.ClassStudents
            .Include(cs => cs.Class)
            .FirstOrDefault(cs => cs.UserId == userId && cs.Class!.CourseId == id);
            
        if (!isAdmin && (enrollment == null || !enrollment.IsPaid))
        {
            TempData["ErrorMessage"] = "Vui lòng thanh toán để truy cập nội dung khóa học.";
            return RedirectToAction("Details", "Courses", new { id = id });
        }

        // Get Chapters and Lessons for this course
        var chapters = _context.CourseChapters
            .Include(c => c.Lessons)
            .Where(c => c.CourseId == id)
            .OrderBy(c => c.OrderIndex)
            .ToList();
            
        // Filter lessons by status manually to handle the conditional logic
        foreach (var chap in chapters)
        {
            chap.Lessons = chap.Lessons
                .Where(l => isAdmin || l.Status == "Approved")
                .OrderBy(l => l.OrderIndex)
                .ToList();
        }

        // Also get lessons NOT in any chapter (orpahns)
        var orphanLessons = _context.Lessons
            .Where(l => l.CourseId == id && l.ChapterId == null && (isAdmin || l.Status == "Approved"))
            .OrderBy(l => l.OrderIndex)
            .ToList();
        
        // Calculate progress percentage accurately (Granular by Quiz Score)
        var allLessons = _context.Lessons.Where(l => l.CourseId == id && (isAdmin || l.Status == "Approved")).ToList();
        double totalProgressScore = 0;
        List<int> completedLessonIds = new List<int>(); // For UI display of checkmarks

        if (allLessons.Any())
        {
            foreach (var lesson in allLessons)
            {
                var hasExam = _context.Exams.Any(ex => ex.LessonId == lesson.Id);
                if (!hasExam)
                {
                    var lp = _context.LearningProgresses.FirstOrDefault(p => p.UserId == userId && p.LessonId == lesson.Id && p.Status == "Completed");
                    if (lp != null) {
                        totalProgressScore += 1.0;
                        completedLessonIds.Add(lesson.Id);
                    }
                }
                else
                {
                    var bestResult = _context.ExamResults
                        .Include(r => r.Attempt)
                        .Where(r => r.Attempt!.UserId == userId && r.Attempt.Exam!.LessonId == lesson.Id)
                        .OrderByDescending(r => (double)r.CorrectCount / r.TotalQuestions)
                        .FirstOrDefault();
                        
                    if (bestResult != null && bestResult.TotalQuestions > 0)
                    {
                        double scoreRatio = (double)bestResult.CorrectCount / bestResult.TotalQuestions;
                        totalProgressScore += scoreRatio;
                        if (scoreRatio >= 1.0) completedLessonIds.Add(lesson.Id);
                    }
                }
            }
        }

        int progressPercent = allLessons.Count > 0 ? (int)(totalProgressScore * 100 / allLessons.Count) : 0;

        ViewBag.Chapters = chapters;
        ViewBag.OrphanLessons = orphanLessons;
        ViewBag.Lessons = allLessons;
        ViewBag.Progress = progressPercent;
        ViewBag.CompletedLessonIds = completedLessonIds;
        ViewBag.TotalCount = allLessons.Count;
        
        return View(course);
    }

    public IActionResult Lesson(int id)
    {
        var lesson = _context.Lessons
            .Include(l => l.Course)
            .FirstOrDefault(l => l.Id == id);

        if (lesson == null) return NotFound();

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userId = int.Parse(userIdString!);
        
        // CHECK PAYMENT STATUS (Admin bypass)
        bool isAdmin = User.IsInRole("Admin");
        var enrollment = _context.ClassStudents
            .Include(cs => cs.Class)
            .FirstOrDefault(cs => cs.UserId == userId && cs.Class!.CourseId == lesson.CourseId);
            
        if (!isAdmin && (enrollment == null || !enrollment.IsPaid))
        {
            TempData["ErrorMessage"] = "Vui lòng thanh toán để truy cập nội dung bài học.";
            return RedirectToAction("Details", "Courses", new { id = lesson.CourseId });
        }

        // Get all course lessons for navigation (Admin sees all)
        var courseLessons = _context.Lessons
            .Where(l => l.CourseId == lesson.CourseId && (isAdmin || l.Status == "Approved"))
            .OrderBy(l => l.OrderIndex)
            .ToList();

        // Get only lessons for the current chapter for the sidebar
        var chapterLessons = _context.Lessons
            .Where(l => l.CourseId == lesson.CourseId && l.ChapterId == lesson.ChapterId)
            .OrderBy(l => l.OrderIndex)
            .ToList();

        // Mark this lesson as viewed
        var progress = _context.LearningProgresses
            .FirstOrDefault(lp => lp.UserId == userId && lp.LessonId == id);
            
        // AUTO-COMPLETE IF NO QUIZ
        bool hasQuiz = _context.Exams.Any(e => e.LessonId == id);
        
        if (progress == null)
        {
            progress = new LearningProgress { 
                UserId = userId, 
                LessonId = id, 
                Status = hasQuiz ? "Incomplete" : "Completed" // Auto complete if no quiz exists
            };
            _context.LearningProgresses.Add(progress);
            _context.SaveChanges();
        }
        else if (!hasQuiz && progress.Status != "Completed")
        {
            progress.Status = "Completed";
            _context.SaveChanges();
        }

        // Navigation based on entire course
        var currentIndex = courseLessons.FindIndex(l => l.Id == id);
        var prevLessonId = currentIndex > 0 ? courseLessons[currentIndex - 1].Id : (int?)null;
        var nextLessonId = currentIndex < courseLessons.Count - 1 ? courseLessons[currentIndex + 1].Id : (int?)null;

        // Chapter Info
        var currentChapter = _context.CourseChapters.FirstOrDefault(c => c.Id == (lesson.ChapterId ?? 0));
        bool isLastInChapter = true;
        if (lesson.ChapterId.HasValue)
        {
            var nextInChapter = chapterLessons.Skip(chapterLessons.FindIndex(l => l.Id == id) + 1).FirstOrDefault();
            if (nextInChapter != null) isLastInChapter = false;
        }

        // Lấy danh sách bài tập (Quiz) liên quan đến bài học này (Admin thấy hết)
        var exams = _context.Exams
            .Where(e => e.LessonId == id && (isAdmin || e.Status == "Approved"))
            .ToList();

        ViewBag.AllLessons = chapterLessons; // Sidebar uses chapter lessons
        ViewBag.ContentHTML = lesson.Content;
        ViewBag.VideoUrl = lesson.VideoUrl;
        ViewBag.PrevId = prevLessonId;
        ViewBag.NextId = nextLessonId;
        ViewBag.Exams = exams;
        ViewBag.IsLastInChapter = isLastInChapter;
        ViewBag.ChapterTitle = currentChapter?.Title ?? "Nội dung chung";
        ViewBag.HasQuiz = hasQuiz;
        
        return View(lesson);
    }

    [HttpPost]
    public IActionResult CompleteLesson(int id)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString)) return Unauthorized();
        var userId = int.Parse(userIdString);

        var progress = _context.LearningProgresses.FirstOrDefault(lp => lp.UserId == userId && lp.LessonId == id);
        
        if (progress == null)
        {
            progress = new LearningProgress { 
                UserId = userId, 
                LessonId = id, 
                Status = "Completed" 
            };
            _context.LearningProgresses.Add(progress);
        }
        else
        {
            progress.Status = "Completed";
        }
        
        _context.SaveChanges();
        return Json(new { success = true });
    }
}
