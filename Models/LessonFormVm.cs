using System.ComponentModel.DataAnnotations;

namespace COTHUYPRO.Models;

public class LessonFormVm
{
    public int? Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public int CourseId { get; set; }

    public int OrderIndex { get; set; }
    
    public IFormFile? DocumentFile { get; set; }
    
    [MaxLength(500)]
    public string? DocumentUrl { get; set; }

    [MaxLength(500)]
    public string? VideoUrl { get; set; }
    
    public string? Content { get; set; }
    
    public int Duration { get; set; }
    
    public bool IsFreePreview { get; set; }

    public bool IsPublished { get; set; } = true;

    public int? BlockId { get; set; }
}
