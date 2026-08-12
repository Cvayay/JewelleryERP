using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace JewelleryERP.Models;

public class Invoice
{
    public int Id { get; set; }
    public string BillNumber { get; set; } = string.Empty; // e.g., Bill No. 344
    public DateTime InvoiceDate { get; set; } = DateTime.Now;

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    // Financial Totals
    public decimal SubTotal { get; set; }
    public decimal MakingChargesTotal { get; set; }
    public decimal CgstAmount { get; set; } // 1.50%
    public decimal SgstAmount { get; set; } // 1.50%
    public decimal GrandTotal { get; set; }

    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();

    [NotMapped]
    public string? CustomerName => Customer?.Name;
}