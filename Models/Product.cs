using System.ComponentModel.DataAnnotations;

namespace JewelleryERP.Models;

public class Product
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Category { get; set; }

    public decimal Price { get; set; }
    public string? HSNCode { get; set; }

    public int StockQuantity { get; set; }

    public DateTime CreatedAt { get; set; }
}
