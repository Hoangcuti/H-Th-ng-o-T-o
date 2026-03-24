using System.ComponentModel.DataAnnotations;

namespace COTHUYPRO.Models;

public class EditInstructorVm
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [EmailAddress]
    [Required]
    public string? Email { get; set; }

    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;
}
