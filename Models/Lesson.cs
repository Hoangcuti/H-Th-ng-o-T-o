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

    public int? ChapterId { get; set; }
    public CourseChapter? Chapter { get; set; }

    [MaxLength(20)]
    public string Status { get; set; } = "Approved";

    public int? CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public int? UpdatedByUserId { get; set; }
    public User? UpdatedByUser { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
