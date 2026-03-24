using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COTHUYPRO.Models;

public class Notification
{
    public int Id { get; set; }
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    public int TypeId { get; set; }
    public NotificationType? Type { get; set; }
}

