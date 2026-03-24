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
}

