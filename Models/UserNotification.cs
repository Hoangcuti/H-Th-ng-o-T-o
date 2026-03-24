using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COTHUYPRO.Models;

public class UserNotification
{
    public int UserId { get; set; }
    public User? User { get; set; }
    public int NotificationId { get; set; }
    public Notification? Notification { get; set; }
}

// ===== Reviews =====
