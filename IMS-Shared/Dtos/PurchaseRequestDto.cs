namespace IMS_Shared.Dtos;

public class PurchaseRequestDto
{
    public string BuyerName { get; set; } = string.Empty;
    public List<PurchaseItemDto> Items { get; set; } = new();
}

public class PurchaseItemDto
{
    public int StockId { get; set; }
    public int Amount { get; set; }
}