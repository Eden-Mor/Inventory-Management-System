using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IMS_Backend.Models;

public class Stock
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? SerialNumber { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal BuyPrice { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal SellPrice { get; set; }

    [Range(0, int.MaxValue)]
    public int Amount { get; set; }

    // Foreign key
    public int SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public ICollection<ItemPurchase> PurchaseItems { get; set; } = [];
}