using System.ComponentModel.DataAnnotations;

namespace COTHUYPRO.Models;

public class LessonFormVm
{
    public int? Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public int CourseId { get; set; }
}
