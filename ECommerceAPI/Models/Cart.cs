namespace ECommerceAPI.Models;

public class Cart
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<CartItem> Items { get; set; } = [];
}
