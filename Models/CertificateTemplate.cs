using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COTHUYPRO.Models;

public class CertificateTemplate
{
    public int Id { get; set; }
    [MaxLength(100)]
    public string TemplateName { get; set; } = string.Empty;
}

