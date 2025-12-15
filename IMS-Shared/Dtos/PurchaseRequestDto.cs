using IMS_Shared.Enums;

namespace IMS_Shared.Dtos;

public class PurchaseRequestDto
{
    public int SellerId { get; set; }
    public PurchaseStatus Status { get; set; } = PurchaseStatus.Purchased;
    public string BuyerName { get; set; } = string.Empty;
    public List<PurchaseRequestItemDto> Items { get; set; } = new();
}

public class PurchaseRequestItemDto
{
    public int StockId { get; set; }
    public int Amount { get; set; }
}