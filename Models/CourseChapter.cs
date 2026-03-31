using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COTHUYPRO.Models;

public class CourseChapter
{
    [Key]
    public int Id { get; set; }
    
    public int CourseId { get; set; }
    public Course? Course { get; set; }
    
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    public int OrderIndex { get; set; }
    
    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}
