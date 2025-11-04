namespace IMS_Shared.Dtos;

public class StockDto
{
    public int? StockId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public decimal BuyPrice { get; set; }
    public decimal SellPrice { get; set; }
    public int SupplierId { get; set; }
    public int? Amount { get; set; }
}