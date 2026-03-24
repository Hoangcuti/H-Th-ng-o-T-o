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
        var courses = _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Level)
            .Include(c => c.Instructors).ThenInclude(i => i.User)
            .ToList();
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
        return View(course);
    }

    [HttpPost]
    [Authorize]
    public IActionResult Enroll(int id)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");
        
        var userId = int.Parse(userIdString);

        var exists = _context.LearningProgress.Any(lp => lp.UserId == userId && lp.CourseId == id);
        if (!exists) 
        {
            var lp = new LearningProgress 
            { 
                UserId = userId, 
                CourseId = id, 
                Percent = 0 
            };
            _context.LearningProgress.Add(lp);
            _context.SaveChanges();
            TempData["SuccessMessage"] = "Đăng ký khóa học thành công! Hệ thống đã ghi nhận doanh thu mới.";
        }
        else 
        {
            TempData["SuccessMessage"] = "Bạn đã đăng ký khóa học này rồi.";
        }

        if (User.IsInRole("Student"))
            return RedirectToAction("CourseDetail", "Student", new { id = id });
        
        return RedirectToAction("Details", new { id = id });
    }
}
