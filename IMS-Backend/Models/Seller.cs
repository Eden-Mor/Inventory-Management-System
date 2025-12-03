using IMS_Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace IMS_Backend.Models;

public class Seller
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    public SellerStatus Status { get; set; } = SellerStatus.Active;

    public List<Purchase> Sales { get; set; } = new();
}