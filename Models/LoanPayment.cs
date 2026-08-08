namespace JewelleryERP.Models;

public class LoanPayment
{
    public int Id { get; set; }

    public int LoanId { get; set; }

    public Loan? Loan { get; set; }

    public DateTime PaymentDate { get; set; }

    public decimal AmountPaid { get; set; }

    public decimal PrincipalCovered { get; set; }

    public decimal InterestCovered { get; set; }

    public string? ReceiptNumber { get; set; }
    
    public string? Notes { get; set; }
}