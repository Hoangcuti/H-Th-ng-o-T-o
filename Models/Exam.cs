using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COTHUYPRO.Models;

public class Exam
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public Course? Course { get; set; }
    public int? LessonId { get; set; }
    public Lesson? Lesson { get; set; }
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public int PassingScore { get; set; }
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public bool ShuffleQuestions { get; set; } = true;
    public int MaxAttempts { get; set; } = 0;
    public bool ShowAnswers { get; set; } = true;
    
    [MaxLength(20)]
    public string ExamType { get; set; } = "Quiz"; // Quiz, Final

    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    public int? CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public int? UpdatedByUserId { get; set; }
    public User? UpdatedByUser { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
