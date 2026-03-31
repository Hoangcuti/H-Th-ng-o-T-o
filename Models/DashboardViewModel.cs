namespace COTHUYPRO.Models;

public class CourseCardVm
{
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public List<string> Instructors { get; set; } = new();
    public int Progress { get; set; }
    public string? ImageUrl { get; set; }
}

public class ClassCardVm
{
    public string CourseName { get; set; } = string.Empty;
    public string Instructor { get; set; } = string.Empty;
    public string Room { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime Schedule { get; set; }
}

public class DashboardViewModel
{
    public List<CourseCardVm> Courses { get; set; } = new();
    public List<ClassCardVm> UpcomingClasses { get; set; } = new();
    public int TotalStudents { get; set; }
    public int TotalInstructors { get; set; }
    public int TotalCourses { get; set; }
    public int ActiveClasses { get; set; }
}
