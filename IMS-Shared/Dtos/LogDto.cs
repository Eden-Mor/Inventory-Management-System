using IMS_Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace IMS_Shared.Dtos;

public class LogDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public LogType TypeEnum { get; set; }
    public string? Description { get; set; }
}

