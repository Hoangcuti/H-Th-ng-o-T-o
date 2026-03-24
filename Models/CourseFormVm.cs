using System.ComponentModel.DataAnnotations;

namespace COTHUYPRO.Models;

public class CourseFormVm
{
    public int? Id { get; set; }

    [Required, MaxLength(200)]
    public string CourseName { get; set; } = string.Empty;

    public int? CategoryId { get; set; }
    public int? LevelId { get; set; }
}
