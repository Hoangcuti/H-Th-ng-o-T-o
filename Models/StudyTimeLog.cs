using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COTHUYPRO.Models;

public class StudyTimeLog
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public int Minutes { get; set; }
}

// ===== Notifications =====
