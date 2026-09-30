using System.ComponentModel.DataAnnotations;

namespace SportEquipmentStore.Web.Models;

public sealed class CartItemInputModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Sản phẩm không hợp lệ.")]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0.")]
    public int Quantity { get; set; } = 1;
}
