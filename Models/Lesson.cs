using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COTHUYPRO.Models;

public class Lesson
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public Course? Course { get; set; }
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    
    [MaxLength(500)]
    public string? DocumentUrl { get; set; }
    
    [MaxLength(500)]
    public string? VideoUrl { get; set; }
    
    public string? Content { get; set; }
    
    public int Duration { get; set; }
    
    public bool IsFreePreview { get; set; }

    public bool IsPublished { get; set; } = true;
}

