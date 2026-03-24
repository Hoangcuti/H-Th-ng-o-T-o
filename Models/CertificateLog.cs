using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COTHUYPRO.Models;

public class CertificateLog
{
    public int Id { get; set; }
    public int CertificateId { get; set; }
    public Certificate? Certificate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// ===== Lesson & course progress =====
