using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JewelleryERP.Models;

public class Loan
{
    public int Id { get; set; }

    [Required]
    public string LoanNumber { get; set; } = string.Empty;

    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public DateTime LoanDate { get; set; }

    public decimal PrincipalAmount { get; set; }

    /// <summary>Monthly interest rate in %, e.g. 2.0 = 2% per month. Copied from Settings on creation.</summary>
    public decimal InterestRate { get; set; }

    /// <summary>Weight in grams (decimal, e.g. 10.500).</summary>
    public decimal Weight { get; set; }

    public decimal PresentValue { get; set; }

    public string? ArticleDescription { get; set; }

    // ── Form-E fields (Pawnbrokers Act) ───────────────────────────────────
    public string? OwnerName { get; set; }
    public string? OwnerAddress { get; set; }

    public string? RedeemerName { get; set; }
    public string? RedeemerAddress { get; set; }

    // ── Lifecycle dates ───────────────────────────────────────────────────
    public DateTime? RedemptionDate { get; set; }

    /// <summary>Set when Status is changed to Auctioned.</summary>
    public DateTime? AuctionDate { get; set; }

    public LoanStatus Status { get; set; } = LoanStatus.Pending;

    public DateTime CreatedAt { get; set; }

    // ── Computed (not stored) ──────────────────────────────────────────────
    [NotMapped]
    public string CustomerName => Customer?.Name ?? string.Empty;

    [NotMapped]
    public int MonthsElapsed =>
        (int)Math.Floor((DateTime.Now - LoanDate).TotalDays / 30.0);

    [NotMapped]
    public decimal InterestAccrued =>
        PrincipalAmount * InterestRate / 100m * MonthsElapsed;

    [NotMapped]
    public decimal TotalDue => PrincipalAmount + InterestAccrued;

    [NotMapped]
    public bool IsOverdue6Months => Status == LoanStatus.Pending && MonthsElapsed >= 6;

    [NotMapped]
    public bool IsOverdue12Months => Status == LoanStatus.Pending && MonthsElapsed >= 12;
}