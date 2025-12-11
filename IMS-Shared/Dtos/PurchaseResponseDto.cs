using System.Text;

namespace IMS_Shared.Dtos;

public class PurchaseResponseDto
{
    public int Id { get; set; }
    public int SellerId { get; set; }
    public DateTime PurchaseDate { get; set; }
    public string BuyerName { get; set; } = string.Empty;
    public List<PurchaseItemResponseDto> Items { get; set; } = [];

    public string ItemList
    {
        get
        {
            StringBuilder sb = new();
            foreach (var item in Items)
                sb.Append($"{item.StockName} ({item.Amount}) | ");

            if (sb.Length > 0)
                sb.Length -= 3;

            return sb.ToString();
        }
    }
}

public class PurchaseItemResponseDto
{
    public string StockName { get; set; } = string.Empty;
    public int Amount { get; set; }
}