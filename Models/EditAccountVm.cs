using System.ComponentModel.DataAnnotations;

namespace COTHUYPRO.Models;

public class EditAccountVm
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(100)]
    public string? Email { get; set; }

    [Required, MaxLength(20)]
    public string Phone { get; set; } = string.Empty;
}
