using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COTHUYPRO.Models;

public class AssignmentSubmission
{
    public int Id { get; set; }
    public int AssignmentId { get; set; }
    public Assignment? Assignment { get; set; }

    public int StudentId { get; set; }
    [ForeignKey("StudentId")]
    public User? Student { get; set; }

    [MaxLength(2000)]
    public string? Content { get; set; }
    
    [MaxLength(500)]
    public string? FileUrl { get; set; } // Student uploaded file
    
    public DateTime SubmittedAt { get; set; } = DateTime.Now;

    // Instructor grading
    public int? Score { get; set; }
    
    [MaxLength(1000)]
    public string? Feedback { get; set; }

    public int? GradedByUserId { get; set; }
    [ForeignKey("GradedByUserId")]
    public User? GradedByUser { get; set; }
    public DateTime? GradedAt { get; set; }
}
