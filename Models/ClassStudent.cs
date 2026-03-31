using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COTHUYPRO.Models;

public class ClassStudent
{
    public int ClassId { get; set; }
    public Class? Class { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public bool IsPaid { get; set; } = false;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal PaidAmount { get; set; } = 0;
    
    public DateTime? PaymentDate { get; set; }
}

