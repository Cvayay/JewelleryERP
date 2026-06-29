using System.ComponentModel.DataAnnotations.Schema;

namespace JewelleryERP.Models;

public class Invoice
{
    public int Id { get; set; }

    public DateTime InvoiceDate { get; set; }

    public int CustomerId { get; set; }

    public Customer? Customer { get; set; }

    public decimal TotalAmount { get; set; }

    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();

    [NotMapped]
    public string? CustomerName => Customer?.Name;
}
