using System.ComponentModel.DataAnnotations;

namespace SportEquipmentStore.Web.Models;

public sealed class CheckoutViewModel
{
    [Required(ErrorMessage = "Vui lòng nhập họ và tên người nhận.")]
    [StringLength(150, ErrorMessage = "Họ và tên không được vượt quá 150 ký tự.")]
    [Display(Name = "Họ và tên người nhận")]
    public string ShippingFullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
    [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự.")]
    [Phone(ErrorMessage = "Số điện thoại không đúng định dạng.")]
    [Display(Name = "Số điện thoại")]
    public string ShippingPhone { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng.")]
    [StringLength(500, ErrorMessage = "Địa chỉ không được vượt quá 500 ký tự.")]
    [Display(Name = "Địa chỉ nhận hàng")]
    public string ShippingAddress { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Ghi chú không được vượt quá 1000 ký tự.")]
    [Display(Name = "Ghi chú")]
    public string? Note { get; set; }

    public Guid CheckoutRequestId { get; set; }

    public IReadOnlyList<CheckoutItemViewModel> Items { get; set; } = [];

    public int TotalItems => Items.Sum(item => item.Quantity);

    public decimal TotalAmount => Items.Sum(item => item.LineTotal);
}
