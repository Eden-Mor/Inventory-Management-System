using System.ComponentModel.DataAnnotations;

namespace IMS_Backend.Models;

public class SupplierOrderItem
{
    public int Id { get; set; }
    public int SupplierOrderId { get; set; }
    public SupplierOrder SupplierOrder { get; set; }

    public int StockId { get; set; }
    public Stock Stock { get; set; }

    public int Amount { get; set; }
}
