using System.ComponentModel.DataAnnotations.Schema;

namespace JewelleryERP.Models;

public class Invoice
{
    public int Id { get; set; }

    /// <summary>Bill number from Settings.CurrentBillNumber, auto-incremented on save.</summary>
    public string BillNumber { get; set; } = string.Empty;

    public DateTime InvoiceDate { get; set; }

    public int CustomerId { get; set; }

    public Customer? Customer { get; set; }

    /// <summary>Sum of all InvoiceItem.MakingCharge values.</summary>
    public decimal MakingChargeTotal { get; set; }

    /// <summary>CGST rate used at time of billing (e.g. 1.5).</summary>
    public decimal CgstRate { get; set; }

    /// <summary>SGST rate used at time of billing (e.g. 1.5).</summary>
    public decimal SgstRate { get; set; }

    /// <summary>Computed: (ItemsSubTotal + MakingChargeTotal) * CgstRate / 100.</summary>
    public decimal CgstAmount { get; set; }

    /// <summary>Computed: (ItemsSubTotal + MakingChargeTotal) * SgstRate / 100.</summary>
    public decimal SgstAmount { get; set; }

    /// <summary>Grand total: ItemsSubTotal + MakingChargeTotal + CgstAmount + SgstAmount.</summary>
    public decimal TotalAmount { get; set; }

    /// <summary>Party/Customer signature placeholder for printed bills (optional).</summary>
    public string? OwnerSignatureField { get; set; }

    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();

    // ── Computed (not stored) ──────────────────────────────────────────────
    [NotMapped]
    public string? CustomerName => Customer?.Name;

    [NotMapped]
    public decimal ItemsSubTotal => Items.Sum(i => i.LineTotal);
}