using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COTHUYPRO.Models;

public class UserProfile
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    [MaxLength(20)]
    public string? Phone { get; set; }
    [MaxLength(255)]
    public string? Address { get; set; }
}

// ===== Course tagging & prerequisites =====
