using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COTHUYPRO.Models;

public class Course
{
    public int Id { get; set; }
    [MaxLength(200)]
    public string CourseName { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public CourseCategory? Category { get; set; }
    public int? LevelId { get; set; }
    public CourseLevel? Level { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; } = 0;

    [MaxLength(1000)]
    public string? ImageUrl { get; set; }

    public ICollection<CourseInstructor> Instructors { get; set; } = new List<CourseInstructor>();
    public ICollection<Class> Classes { get; set; } = new List<Class>();
}
