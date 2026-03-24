using System.ComponentModel.DataAnnotations;

namespace COTHUYPRO.Models;

public class ProfileViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ tên")]
    [Display(Name = "Họ tên")]
    public string FullName { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập Email")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    [Display(Name = "Số điện thoại")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    public string? Phone { get; set; }

    [Display(Name = "Mật khẩu mới (bỏ trống nếu không đổi)")]
    [DataType(DataType.Password)]
    [MinLength(3, ErrorMessage = "Mật khẩu tối thiểu 3 ký tự")]
    public string? NewPassword { get; set; }

    [Display(Name = "Xác nhận mật khẩu mới")]
    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
    public string? ConfirmNewPassword { get; set; }

    [Display(Name = "Mã sinh viên")]
    public string? StudentCode { get; set; }
    public List<string> EnrolledCourses { get; set; } = new();
}
