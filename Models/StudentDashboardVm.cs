namespace COTHUYPRO.Models;

public class StudentDashboardVm
{
    public List<LearningProgress> Learning { get; set; } = new();
    public List<Class> Classes { get; set; } = new();
}
