using System.ComponentModel.DataAnnotations;

namespace COTHUYPRO.Models;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Nhập tên đăng nhập")]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nhập họ tên")]
    [MaxLength(150)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nhập email")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [MaxLength(100)]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Nhập số điện thoại")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nhập mật khẩu")]
    [DataType(DataType.Password)]
    [MinLength(3)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Mật khẩu nhập lại không khớp")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
