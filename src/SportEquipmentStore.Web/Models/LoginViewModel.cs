using System.ComponentModel.DataAnnotations;

namespace SportEquipmentStore.Web.Models;

public sealed class LoginViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập hoặc email.")]
    [StringLength(254, ErrorMessage = "Thông tin đăng nhập không được vượt quá 254 ký tự.")]
    [Display(Name = "Tên đăng nhập hoặc email")]
    public string Identifier { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Ghi nhớ đăng nhập")]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }
}
