using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COTHUYPRO.Models;

public class User
{
    public int Id { get; set; }
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;
    [MaxLength(255)]
    public string Password { get; set; } = string.Empty;
    [MaxLength(100)]
    public string? Email { get; set; }
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;
    public int? DepartmentId { get; set; }
    public Department? Department { get; set; }
    public int? PositionId { get; set; }
    public Position? Position { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    [MaxLength(20)]
    public string? StudentCode { get; set; }
    public ICollection<UserProfile> UserProfiles { get; set; } = new List<UserProfile>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<CourseInstructor> TeachingCourses { get; set; } = new List<CourseInstructor>();
    public ICollection<Class> Classes { get; set; } = new List<Class>();
    public ICollection<LearningProgress> LearningProgresses { get; set; } = new List<LearningProgress>();
}

