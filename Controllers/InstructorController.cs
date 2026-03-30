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

        ViewBag.AvailableCourses = _context.Courses
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
}
