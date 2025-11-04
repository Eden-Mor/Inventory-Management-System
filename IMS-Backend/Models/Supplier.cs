using System.ComponentModel.DataAnnotations;

namespace IMS_Backend.Models;

public class Supplier
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    // Navigation property
    public ICollection<Stock> Stocks { get; set; } = new List<Stock>();
}