namespace COTHUYPRO.Models;

public class InstructorDashboardVm
{
    public List<Course> Courses { get; set; } = new();
    public List<Class> Classes { get; set; } = new();
    public List<User> Students { get; set; } = new();
}
