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

        // Tìm lớp học đầu tiên của khóa này đang mở (StatusId=1) hoặc bất kỳ lớp nào
        var targetClass = _context.Classes.FirstOrDefault(c => c.CourseId == id);
        if (targetClass == null)
        {
            TempData["Error"] = "Khóa học hiện chưa có lớp học nào mở.";
            return RedirectToAction("Details", new { id = id });
        }

        var exists = _context.ClassStudents.Any(cs => cs.UserId == userId && cs.ClassId == targetClass.Id);
        if (!exists) 
        {
            var cs = new ClassStudent 
            { 
                UserId = userId, 
                ClassId = targetClass.Id 
            };
            _context.ClassStudents.Add(cs);
            _context.SaveChanges();
            TempData["SuccessMessage"] = "Đăng ký khóa học thành công! Bạn hiện đã có tên trong danh sách lớp.";
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
