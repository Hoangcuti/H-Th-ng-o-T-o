using COTHUYPRO.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace COTHUYPRO.Controllers;

[Authorize]
public class PaymentController : Controller
{
    private readonly TrainingContext _context;

    public PaymentController(TrainingContext context)
    {
        _context = context;
    }

    public IActionResult Checkout(int courseId)
    {
        var course = _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Level)
            .FirstOrDefault(c => c.Id == courseId);

        if (course == null) return NotFound();

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        // Kiểm tra xem đã đăng ký chưa
        var enrollment = _context.ClassStudents
            .Include(cs => cs.Class)
            .FirstOrDefault(cs => cs.UserId == userId && cs.Class!.CourseId == courseId);

        if (enrollment != null && enrollment.IsPaid)
        {
            return RedirectToAction("CourseDetail", "Student", new { id = courseId });
        }

        return View(course);
    }

    [HttpPost]
    public IActionResult ProcessPayment(int courseId, string method)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        
        // Tìm enrollment của khóa học này (thông qua Class)
        var enrollment = _context.ClassStudents
            .Include(cs => cs.Class)
            .FirstOrDefault(cs => cs.UserId == userId && cs.Class!.CourseId == courseId);

        if (enrollment == null)
        {
            // Nếu chưa nhấn Enroll ở trang Details thì tự động tạo record pending
            var targetClass = _context.Classes.FirstOrDefault(c => c.CourseId == courseId);
            if (targetClass == null) return NotFound();

            enrollment = new ClassStudent { UserId = userId, ClassId = targetClass.Id, IsPaid = false };
            _context.ClassStudents.Add(enrollment);
        }

        var course = _context.Courses.Find(courseId);
        if (course == null) return NotFound();

        // SIMULATE SUCCESSFUL PAYMENT
        enrollment.IsPaid = true;
        enrollment.PaidAmount = course.Price;
        enrollment.PaymentDate = DateTime.Now;

        _context.SaveChanges();

        TempData["SuccessMessage"] = $"Thanh toán khóa học '{course.CourseName}' thành công!";
        return RedirectToAction("CourseDetail", "Student", new { id = courseId });
    }
}
