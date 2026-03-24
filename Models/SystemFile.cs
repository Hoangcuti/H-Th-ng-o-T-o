using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COTHUYPRO.Models;

[Table("Files")]
public class SystemFile
{
    public int Id { get; set; }
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;
    public int FileTypeId { get; set; }
    public FileType? FileType { get; set; }
}

