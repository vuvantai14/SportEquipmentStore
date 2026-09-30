using System.ComponentModel.DataAnnotations;

namespace SportEquipmentStore.Web.Models;

public sealed class RegisterViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải có từ 3 đến 50 ký tự.")]
    [Display(Name = "Tên đăng nhập")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập email.")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
    [StringLength(254, ErrorMessage = "Email không được vượt quá 254 ký tự.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
    [StringLength(150, ErrorMessage = "Họ và tên không được vượt quá 150 ký tự.")]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Mật khẩu phải có từ 8 đến 100 ký tự.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu.")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    [Display(Name = "Xác nhận mật khẩu")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
