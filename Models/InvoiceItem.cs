using System.ComponentModel.DataAnnotations.Schema;

namespace JewelleryERP.Models;

public class InvoiceItem
{
    public int Id { get; set; }

    public int InvoiceId { get; set; }
    public Invoice? Invoice { get; set; }

    public int ProductId { get; set; }
    public Product? Product { get; set; }

    /// <summary>Free-text description of the jewellery article, e.g. "Gold Ring 22ct".</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>HSN code for the article, e.g. 7113.</summary>
    public string HsnCode { get; set; } = string.Empty;

    // ── Weight fields (matching the physical bill: Gms + Mg) ──────────────
    public decimal GrossWeightGms { get; set; }
    public decimal GrossWeightMg { get; set; }
    public decimal NetWeightGms { get; set; }
    public decimal NetWeightMg { get; set; }

    /// <summary>Rate per gram at time of billing, auto-filled from Settings.</summary>
    public decimal RatePerGram { get; set; }

    /// <summary>Making / labour charge for this specific item.</summary>
    public decimal MakingCharge { get; set; }

    /// <summary>Piece count — defaults to 1. Use for pairs/sets (e.g. earrings = 2).</summary>
    public int Quantity { get; set; } = 1;

    // ── Preserved for backward compat ─────────────────────────────────────
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// LineTotal = (NetWeightGms + NetWeightMg / 1000) * RatePerGram
    /// Making charge is totalled at invoice level, not per line.
    /// </summary>
    public decimal LineTotal { get; set; }

    // ── Computed (not stored) ──────────────────────────────────────────────
    [NotMapped]
    public string? ProductName { get; set; }

    [NotMapped]
    public decimal NetWeightTotal => NetWeightGms + NetWeightMg / 1000m;

    [NotMapped]
    public decimal GrossWeightTotal => GrossWeightGms + GrossWeightMg / 1000m;
}