using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COTHUYPRO.Models;

public class LearningProgress
{
    public int UserId { get; set; }
    public User? User { get; set; }

    public int LessonId { get; set; }
    public Lesson? Lesson { get; set; }

    [Column("Completed")]
    public bool Completed { get; set; }

    [Column("CompletionDate")]
    public DateTime? CompletionDate { get; set; }
}
