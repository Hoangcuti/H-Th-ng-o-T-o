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
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        // Lấy tiến độ các bài học
        var learning = _context.LearningProgress
            .Include(lp => lp.Lesson).ThenInclude(l => l!.Course)
            .Where(lp => lp.UserId == userId)
            .ToList();

        var classes = _context.ClassStudents
            .Include(cs => cs.Class)!.ThenInclude(c => c.Course)
            .Include(cs => cs.Class)!.ThenInclude(c => c.Room)
            .Include(cs => cs.Class)!.ThenInclude(c => c.Status)
            .Include(cs => cs.Class)!.ThenInclude(c => c.Schedules)
            .Where(cs => cs.UserId == userId)
            .Select(cs => cs.Class!)
            .Distinct()
            .ToList();

        var vm = new StudentDashboardVm
        {
            Learning = learning,
            Classes = classes
        };
        return View(vm);
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

        // Get lessons for this course
        var lessons = _context.Lessons
            .Where(l => l.CourseId == id)
            .ToList();

        // Calculate progress percentage
        int totalLessons = lessons.Count;
        int completedLessons = 0;
        if (totalLessons > 0)
        {
            var lessonIds = lessons.Select(l => l.Id).ToList();
            completedLessons = _context.LearningProgress
                .Count(lp => lp.UserId == userId && lessonIds.Contains(lp.LessonId) && lp.Completed);
        }

        ViewBag.Lessons = lessons;
        ViewBag.Progress = totalLessons > 0 ? (completedLessons * 100 / totalLessons) : 0;
        ViewBag.CompletedCount = completedLessons;
        ViewBag.TotalCount = totalLessons;

        return View(course);
    }

    public IActionResult Lesson(int id)
    {
        var lesson = _context.Lessons
            .Include(l => l.Course)
            .FirstOrDefault(l => l.Id == id);

        if (lesson == null) return NotFound();

        // Get all lessons for the sidebar
        var allLessons = _context.Lessons
            .Where(l => l.CourseId == lesson.CourseId)
            .ToList();

        var lessonContent = _context.Set<LessonContent>().FirstOrDefault(lc => lc.LessonId == id);
        var lessonVideo = _context.Set<LessonVideo>().FirstOrDefault(lv => lv.LessonId == id);

        ViewBag.AllLessons = allLessons;
        ViewBag.ContentHTML = lessonContent?.Content;
        ViewBag.VideoUrl = lessonVideo?.Url;
        return View(lesson);
    }
}
