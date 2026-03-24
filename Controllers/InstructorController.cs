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
        return View(vm);
    }
}
