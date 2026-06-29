using System.ComponentModel.DataAnnotations.Schema;

namespace JewelleryERP.Models;

public class InvoiceItem
{
    public int Id { get; set; }

    public int InvoiceId { get; set; }

    public Invoice? Invoice { get; set; }

    public int ProductId { get; set; }

    public Product? Product { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal LineTotal { get; set; }

    [NotMapped]
    public string? ProductName { get; set; }
}
