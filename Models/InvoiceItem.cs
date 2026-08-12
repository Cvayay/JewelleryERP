using System.ComponentModel.DataAnnotations.Schema;

namespace JewelleryERP.Models;

public class InvoiceItem
{
    public int Id { get; set; }

    public int InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public string Description { get; set; } = string.Empty; // Description of Goods
    public string HsnCode { get; set; } = string.Empty;

    // Jewellery Weights (Grams / Mg)
    public decimal GrossWeight { get; set; }
    public decimal NetWeight { get; set; }

    // Rate & Calculations
    public decimal RatePerGram { get; set; }
    public decimal MakingCharges { get; set; } // MC
    public decimal LineTotal { get; set; }     // (NetWeight * RatePerGram) + MakingCharges

    [NotMapped]
    public string? ProductName => Product?.Name;
}