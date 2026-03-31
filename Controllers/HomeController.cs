using System.Diagnostics;
using COTHUYPRO.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace COTHUYPRO.Controllers;

public class HomeController : Controller
{
    private readonly TrainingContext _context;

    public HomeController(TrainingContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var courses = _context.Courses
            .Include(c => c.Category)
            .Include(c => c.Level)
            .Include(c => c.Instructors).ThenInclude(ci => ci.User)
            .ToList();

        var classes = _context.Classes
            .Include(c => c.Course)
            .Include(c => c.Instructor)
            .Include(c => c.Room)
            .Include(c => c.Status)
            .Include(c => c.Schedules)
            .ToList();

        var vm = new DashboardViewModel
        {
            Courses = courses.Select(c => new CourseCardVm
            {
                Title = c.CourseName,
                Category = c.Category?.Name ?? "N/A",
                Level = c.Level?.Name ?? "N/A",
                Instructors = c.Instructors.Select(i => i.User?.FullName ?? i.User?.Username ?? "").Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList(),
                Progress = 0,
                ImageUrl = c.ImageUrl
            }).ToList(),
            UpcomingClasses = classes
                .Select(cls => new ClassCardVm
                {
                    CourseName = cls.Course?.CourseName ?? "Lớp",
                    Instructor = cls.Instructor?.FullName ?? cls.Instructor?.Username ?? "N/A",
                    Room = cls.Room?.Name ?? "TBD",
                    Status = cls.Status?.Name ?? "N/A",
                    Schedule = cls.Schedules.OrderBy(s => s.ScheduleDate).FirstOrDefault()?.ScheduleDate ?? DateTime.UtcNow.AddDays(1)
                })
                .OrderBy(c => c.Schedule)
                .Take(6)
                .ToList(),
            TotalCourses = courses.Count,
            TotalStudents = _context.UserRoles.Include(x => x.Role).Count(x => x.Role!.Name == "Student"),
            TotalInstructors = _context.UserRoles.Include(x => x.Role).Count(x => x.Role!.Name == "Instructor"),
            ActiveClasses = classes.Count
        };

        return View(vm);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
