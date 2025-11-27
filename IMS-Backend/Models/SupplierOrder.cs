using IMS_Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace IMS_Backend.Models;

public class SupplierOrder
{
    public int Id { get; set; }
    public int SupplierId { get; set; }
    public Supplier Supplier { get; set; }

    public DateTime CreatedDate { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime? StatusChangeDate { get; private set; }

    public List<SupplierOrderItem> Items { get; set; } = [];

    public void SetStatus(OrderStatus newStatus)
    {
        Status = newStatus;
        StatusChangeDate = DateTime.UtcNow;
    }
}