using System.ComponentModel.DataAnnotations;

namespace IMS_Backend.Models;


public class Purchase
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string BuyerName { get; set; } = string.Empty;

    public ICollection<ItemPurchase> Items { get; set; } = [];

    public int SellerId { get; set; }
    public Seller Seller { get; set; }

    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
}