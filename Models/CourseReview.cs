using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COTHUYPRO.Models;

public class CourseReview
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public Course? Course { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public int RatingId { get; set; }
    public Rating? Rating { get; set; }
}

