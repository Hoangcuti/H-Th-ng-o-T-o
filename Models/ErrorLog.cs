using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COTHUYPRO.Models;

public class ErrorLog
{
    public int Id { get; set; }
    public string? Message { get; set; }
}

