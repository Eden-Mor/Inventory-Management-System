using IMS_Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace IMS_Shared.Dtos;

public class SellerDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public SellerStatus Status { get; set; }
}