using System.ComponentModel.DataAnnotations;

namespace JewelleryERP.Models;

public class Customer
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }

    public DateTime CreatedAt { get; set; }
}
