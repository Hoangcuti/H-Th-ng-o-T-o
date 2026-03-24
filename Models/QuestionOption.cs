using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COTHUYPRO.Models;

public class QuestionOption
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public Question? Question { get; set; }
    [MaxLength(255)]
    public string Content { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}

