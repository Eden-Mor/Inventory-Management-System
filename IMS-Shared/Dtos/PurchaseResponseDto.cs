using IMS_Shared.Enums;
using System.Text;

namespace IMS_Shared.Dtos;

public class PurchaseResponseDto
{
    public int Id { get; set; }
    public int SellerId { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public string BuyerName { get; set; } = string.Empty;
    public PurchaseStatus Status { get; set; }
    public List<PurchaseItemResponseDto> Items { get; set; } = [];

    private string? _itemList;
    public string ItemList => _itemList ??= string.Join("\n", Items.Select(i => $"{i.StockName} ({i.Amount})"));
}

public class PurchaseItemResponseDto
{
    public string StockName { get; set; } = string.Empty;
    public int Amount { get; set; }
}