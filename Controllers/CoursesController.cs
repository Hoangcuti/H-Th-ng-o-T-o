using COTHUYPRO.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace COTHUYPRO.Controllers;

public class CoursesController : Controller
{
    private readonly TrainingContext _context;
    public CoursesController(TrainingContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        List<int> enrolledCourseIds = new();
        bool isAdmin = User.IsInRole("Admin");

        if (!string.IsNullOrEmpty(userIdString))
        {
            var userId = int.Parse(userIdString);
            enrolledCourseIds = _context.ClassStudents
                .Include(cs => cs.Class)
                .Where(cs => cs.UserId == userId && cs.Class != null && cs.Class.CourseId.HasValue)
                .Select(cs => cs.Class!.CourseId!.Value)
                .ToList();
        }

        var coursesQuery = _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Level)
            .Include(c => c.Instructors).ThenInclude(i => i.User)
            .AsQueryable();

        // Admin sees all, even if enrolled. Students see only not-yet-enrolled.
        if (!isAdmin)
        {
            coursesQuery = coursesQuery.Where(c => !enrolledCourseIds.Contains(c.Id));
        }

        var courses = coursesQuery.ToList();
        ViewBag.EnrolledCourseIds = enrolledCourseIds;
        ViewBag.IsAdmin = isAdmin;
        
        return View(courses);
    }

    public IActionResult Details(int id)
    {
        var course = _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Level)
            .Include(c => c.Instructors).ThenInclude(i => i.User)
            .FirstOrDefault(c => c.Id == id);
        if (course == null) return NotFound();

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        bool isEnrolled = false;
        bool isAdmin = User.IsInRole("Admin");

        if (!string.IsNullOrEmpty(userIdString))
        {
            var userId = int.Parse(userIdString);
            isEnrolled = _context.ClassStudents
                .Any(cs => cs.UserId == userId && cs.Class != null && cs.Class.CourseId == id);
        }

        ViewBag.IsEnrolled = isEnrolled;
        ViewBag.IsAdmin = isAdmin;

        // Pass approved lessons to view (Admin sees all)
        ViewBag.Lessons = _context.Lessons
            .Where(l => l.CourseId == id && (isAdmin || l.Status == "Approved"))
            .OrderBy(l => l.OrderIndex)
            .ToList();

        return View(course);
    }

    [HttpPost]
    [Authorize]
    public IActionResult Enroll(int id)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");
        
        var userId = int.Parse(userIdString);
        bool isAdmin = User.IsInRole("Admin");

        // Admin bypass registration, redirect straight to detail
        if (isAdmin) return RedirectToAction("CourseDetail", "Student", new { id = id });

        // Tìm lớp học đầu tiên của khóa này
        var targetClass = _context.Classes.FirstOrDefault(c => c.CourseId == id);
        
        if (targetClass == null)
        {
            targetClass = new Class { CourseId = id, StatusId = 1 };
            _context.Classes.Add(targetClass);
            _context.SaveChanges();
        }

        var exists = _context.ClassStudents.Any(cs => cs.UserId == userId && cs.ClassId == targetClass.Id);
        if (!exists) 
        {
            var cs = new ClassStudent 
            { 
                UserId = userId, 
                ClassId = targetClass.Id,
                IsPaid = false,
                CreatedAt = DateTime.Now
            };
            _context.ClassStudents.Add(cs);
            _context.SaveChanges();
        }

        return RedirectToAction("Checkout", "Payment", new { courseId = id });
    }

    [HttpPost]
    [Authorize]
    public IActionResult EnrollByClassCode(string classCode)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");
        if (string.IsNullOrWhiteSpace(classCode))
        {
            TempData["Error"] = "Please enter a valid class code.";
            return RedirectToAction(nameof(Index));
        }

        var userId = int.Parse(userIdString);
        var targetClass = _context.Classes
            .Include(c => c.Course)
            .FirstOrDefault(c => c.ClassCode == classCode.Trim());

        if (targetClass == null || targetClass.Course == null)
        {
            TempData["Error"] = "Invalid class code. Please check and try again.";
            return RedirectToAction(nameof(Index));
        }

        var exists = _context.ClassStudents.Any(cs => cs.UserId == userId && cs.ClassId == targetClass.Id);
        if (!exists)
        {
            var cs = new ClassStudent
            {
                UserId = userId,
                ClassId = targetClass.Id,
                IsPaid = false,
                CreatedAt = DateTime.Now
            };
            _context.ClassStudents.Add(cs);
            _context.SaveChanges();
        }

        return RedirectToAction("Details", new { id = targetClass.CourseId });
    }
}
