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
}

