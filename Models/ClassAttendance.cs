using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COTHUYPRO.Models;

public class ClassAttendance
{
    public int Id { get; set; }
    public int ClassId { get; set; }
    public Class? Class { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    [MaxLength(50)]
    public string Status { get; set; } = string.Empty;
}

// ===== Lessons =====
