using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COTHUYPRO.Models;

public class InstructorReview
{
    public int Id { get; set; }
    public int InstructorId { get; set; }
    public User? Instructor { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public int RatingId { get; set; }
    public Rating? Rating { get; set; }
}

