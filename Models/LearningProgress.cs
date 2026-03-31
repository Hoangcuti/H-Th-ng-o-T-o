using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COTHUYPRO.Models;

public class LearningProgress
{
    [Key]
    public int Id { get; set; }
    
    public int UserId { get; set; }
    public User? User { get; set; }

    public int LessonId { get; set; }
    public Lesson? Lesson { get; set; }

    [Column("Status")]
    public string Status { get; set; } = "Incomplete";

}
