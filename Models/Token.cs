using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COTHUYPRO.Models;

public class Token
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    [MaxLength(255)]
    public string TokenValue { get; set; } = string.Empty;
}
