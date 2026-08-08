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

    public decimal Weight { get; set; }
    public ICollection<LoanPayment> Payments { get; set; } = new List<LoanPayment>();

    public decimal PresentValue { get; set; }

    public string? ArticleDescription { get; set; }

    public string? OwnerName { get; set; }

    public string? OwnerAddress { get; set; }

    public string? RedeemerName { get; set; }

    public string? RedeemerAddress { get; set; }

    public DateTime? RedemptionDate { get; set; }

    public LoanStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
}
