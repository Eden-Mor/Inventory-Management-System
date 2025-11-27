namespace IMS_Shared.Dtos;

public class CreateSupplierOrderDto
{
    public int SupplierId { get; set; }
    public List<OrderItemDto> Items { get; set; } = [];
}

public class OrderItemDto
{
    public int StockId { get; set; }
    public int Amount { get; set; }
}
