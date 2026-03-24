using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COTHUYPRO.Models;

public class Exam
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public Course? Course { get; set; }
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public int PassingScore { get; set; }
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public bool ShuffleQuestions { get; set; } = true;
    public int MaxAttempts { get; set; } = 0;
    public bool ShowAnswers { get; set; } = true;
}

