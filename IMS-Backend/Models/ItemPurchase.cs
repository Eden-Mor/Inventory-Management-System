using System.ComponentModel.DataAnnotations;

namespace IMS_Backend.Models;

public class ItemPurchase
{
    public int Id { get; set; }

    public int PurchaseId { get; set; }
    public Purchase? Purchase { get; set; }

    public int StockId { get; set; }
    public Stock? Stock { get; set; }

    [Range(1, int.MaxValue)]
    public int Amount { get; set; }
}