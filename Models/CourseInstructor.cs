using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COTHUYPRO.Models;

public class CourseInstructor
{
    public int CourseId { get; set; }
    public Course? Course { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
}

