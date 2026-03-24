using System.ComponentModel.DataAnnotations;

namespace COTHUYPRO.Models;

public class AdminCourseRow
{
    public Course Course { get; set; } = null!;
    public string CategoryName { get; set; } = string.Empty;
    public string LevelName { get; set; } = string.Empty;
    public int ClassCount { get; set; }
    public int LearnerCount { get; set; }
    public List<string> Instructors { get; set; } = new();
}

public class CoursesPageVm
{
    public List<AdminCourseRow> Courses { get; set; } = new();
}

public class UserAccountVm
{
    public User User { get; set; } = null!;
    public List<string> Roles { get; set; } = new();
}

public class AccountsPageVm
{
    public List<UserAccountVm> Accounts { get; set; } = new();
    public List<Role> Roles { get; set; } = new();
    public List<CourseCategory> Categories { get; set; } = new();
}

public class StudentVm
{
    public User User { get; set; } = null!;
    public string Department { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public int CompletedCourses { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public List<string> EnrolledCourses { get; set; } = new();
}

public class StudentsPageVm
{
    public List<StudentVm> Students { get; set; } = new();
}

public class InstructorVm
{
    public User User { get; set; } = null!;
    public int CourseCount { get; set; }
}

public class InstructorsPageVm
{
    public List<InstructorVm> Instructors { get; set; } = new();
    public List<Course> Courses { get; set; } = new();
}

public class QuizPageVm
{
    public List<Exam> Exams { get; set; } = new();
}

public class PaymentPageVm
{
    public List<Course> Courses { get; set; } = new();
}

public class LessonsPageVm
{
    public List<LessonRow> Lessons { get; set; } = new();
}

public class LessonRow
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
}

public class RevenueCourseVm
{
    public string CourseName { get; set; } = string.Empty;
    public int Learners { get; set; }
    public decimal Revenue { get; set; }
}

public class RevenuePageVm
{
    public int TotalLearners { get; set; }
    public int TotalCourses { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<RevenueCourseVm> TopCourses { get; set; } = new();
    public List<decimal> MonthlyRevenue { get; set; } = new();
}
