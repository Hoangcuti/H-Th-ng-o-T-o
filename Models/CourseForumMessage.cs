using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COTHUYPRO.Models;

public class CourseForumMessage
{
    public int Id { get; set; }
    
    public int CourseId { get; set; }
    public Course? Course { get; set; }
    
    public int UserId { get; set; }
    public User? User { get; set; }
    
    [Required]
    [MaxLength(2000)]
    public string Content { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    // Optional: For replies in the future
    public int? ParentMessageId { get; set; }
    public CourseForumMessage? ParentMessage { get; set; }
}
