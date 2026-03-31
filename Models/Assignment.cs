using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COTHUYPRO.Models;

public class Assignment
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public Course? Course { get; set; }
    public int? LessonId { get; set; }
    public Lesson? Lesson { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string? Description { get; set; }
    
    [MaxLength(500)]
    public string? FileUrl { get; set; } // Instructor provided file

    public DateTime? DueDate { get; set; }
    public int MaxScore { get; set; } = 100;

    [MaxLength(20)]
    public string AssignmentType { get; set; } = "Regular";

    [MaxLength(20)]
    public string Status { get; set; } = "Approved";

    public int? CreatedByUserId { get; set; }
    [ForeignKey("CreatedByUserId")]
    public User? CreatedByUser { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public int? UpdatedByUserId { get; set; }
    [ForeignKey("UpdatedByUserId")]
    public User? UpdatedByUser { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
