using IMS_Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace IMS_Shared.Dtos;

public class SupplierOrderDto
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public OrderStatus Status { get; set; }
    public SupplierDto Supplier { get; set; }
    public List<StockDto> Items { get; set; } = [];
    public DateTime? StatusChangeDate { get; set; }
}