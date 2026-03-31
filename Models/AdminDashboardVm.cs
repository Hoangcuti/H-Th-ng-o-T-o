namespace COTHUYPRO.Models;

public class AdminDashboardVm
{
    public int UserCount { get; set; }
    public int CourseCount { get; set; }
    public int ClassCount { get; set; }
    public int ExamCount { get; set; }
    public int PendingCertificates { get; set; }
    public int SoldCourseCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public double AvgStudentProgress { get; set; }
}

public class AdminIndexViewModel
{
    public AdminDashboardVm Stats { get; set; } = new();
    public List<User> RecentUsers { get; set; } = new();
    public List<Course> RecentCourses { get; set; } = new();
    public List<RevenueCourseVm> TopCourses { get; set; } = new();

    // Chart Data
    public string[] RevenueLabels { get; set; } = Array.Empty<string>();
    public decimal[] RevenueData { get; set; } = Array.Empty<decimal>();
    public string[] CourseLabels { get; set; } = Array.Empty<string>();
    public int[] CourseData { get; set; } = Array.Empty<int>();
}
