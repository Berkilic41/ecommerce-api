namespace ECommerceAPI.Models;

public class CartItem
{
    public int Id { get; set; }
    public int CartId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string? ProductName { get; set; }
    public decimal? ProductPrice { get; set; }
    public string? ProductImageUrl { get; set; }
    public int? StockQuantity { get; set; }
}
