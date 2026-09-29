namespace SportEquipmentStore.Core.Entities;

public class Customer
{
    public int CustomerId { get; set; }

    public int UserId { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public User User { get; set; } = null!;

    public Cart? Cart { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
