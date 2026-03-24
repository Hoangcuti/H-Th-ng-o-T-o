using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COTHUYPRO.Models;

public class Lesson
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public Course? Course { get; set; }
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
}

