using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COTHUYPRO.Models;

public class Class
{
    public int Id { get; set; }
    public int? CourseId { get; set; }
    public Course? Course { get; set; }
    public int? InstructorId { get; set; }
    public User? Instructor { get; set; }
    public int? RoomId { get; set; }
    public ClassRoom? Room { get; set; }
    public int? StatusId { get; set; }
    public ClassStatus? Status { get; set; }
    public ICollection<ClassSchedule> Schedules { get; set; } = new List<ClassSchedule>();
    public ICollection<ClassStudent> ClassStudents { get; set; } = new List<ClassStudent>();
}
