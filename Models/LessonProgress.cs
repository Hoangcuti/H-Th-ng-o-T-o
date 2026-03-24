using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COTHUYPRO.Models;

public class LessonProgress
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public int LessonId { get; set; }
    public Lesson? Lesson { get; set; }
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;
}

