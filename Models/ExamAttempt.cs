using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COTHUYPRO.Models;

public class ExamAttempt
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public int ExamId { get; set; }
    public Exam? Exam { get; set; }

    // Persistence & Flow
    public int RemainingSeconds { get; set; }
    public string? AnswersJson { get; set; } // Stores current selections
    public string Status { get; set; } = "Started"; // Started, Finished
    public DateTime StartedAt { get; set; } = DateTime.Now;
    public DateTime? CompletedAt { get; set; }
}

