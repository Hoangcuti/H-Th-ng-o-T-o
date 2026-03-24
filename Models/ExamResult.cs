using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COTHUYPRO.Models;

public class ExamResult
{
    public int Id { get; set; }
    public int AttemptId { get; set; }
    public ExamAttempt? Attempt { get; set; }
    public int Score { get; set; }
}

