using System.ComponentModel.DataAnnotations;

namespace COTHUYPRO.Models;

public class ExamFormVm
{
    public int? Id { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public int CourseId { get; set; }

    public int? LessonId { get; set; }

    public int DurationMinutes { get; set; } = 15;
    
    public int PassingScore { get; set; } = 50;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool ShuffleQuestions { get; set; } = true;
    public int MaxAttempts { get; set; } = 0;
    public bool ShowAnswers { get; set; } = true;
    public string ExamType { get; set; } = "Quiz";
}
