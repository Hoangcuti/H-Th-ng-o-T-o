using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COTHUYPRO.Models;

public class ExamResult
{
    public int Id { get; set; }
    public int AttemptId { get; set; }
    public ExamAttempt? Attempt { get; set; }
    public double Score { get; set; } // Scale 10.0
    public int CorrectCount { get; set; }
    public int TotalQuestions { get; set; }
}

