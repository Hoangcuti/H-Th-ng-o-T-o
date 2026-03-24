using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COTHUYPRO.Models;

public class Question
{
    public int Id { get; set; }
    public int ExamId { get; set; }
    public Exam? Exam { get; set; }
    public string? Content { get; set; }
}

