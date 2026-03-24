using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COTHUYPRO.Models;

public class UserCertificate
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public int CertificateId { get; set; }
    public Certificate? Certificate { get; set; }
}

