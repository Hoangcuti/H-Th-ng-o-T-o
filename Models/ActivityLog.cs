using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COTHUYPRO.Models;

public class ActivityLog
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public User? User { get; set; }
    [MaxLength(255)]
    public string Action { get; set; } = string.Empty;
}

