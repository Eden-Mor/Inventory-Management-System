using System.ComponentModel.DataAnnotations;

namespace IMS_Backend.Models;

public class Log
{
    public int Id { get; set; }

    public DateTime Date { get; set; } = DateTime.UtcNow;

    public LogType TypeEnum { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }
}

public enum LogType
{
    Default = 0,
    Stock_Added = 1,
    Stock_Sold = 2,
    Added_Item = 3,
    Edited_Item = 4,
    Removed_Item = 5,
    Supplier_Added = 6,
}