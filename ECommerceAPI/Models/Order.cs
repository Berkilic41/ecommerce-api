namespace ECommerceAPI.Models;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Pending";
    public string ShippingAddress { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<OrderItem> Items { get; set; } = [];
}
