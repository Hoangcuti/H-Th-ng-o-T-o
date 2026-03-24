using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COTHUYPRO.Models;

[Table("CourseTagMapping")]
public class CourseTagMapping
{
    public int CourseId { get; set; }
    public Course? Course { get; set; }
    public int TagId { get; set; }
    public CourseTag? Tag { get; set; }
}

