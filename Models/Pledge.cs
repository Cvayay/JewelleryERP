namespace JewelleryERP.Models
{
    public class Pledge
    {
        public int PledgeId { get; set; }
        public string PledgeNumber { get; set; } = string.Empty;
        public DateTime PledgeDate { get; set; } = DateTime.Now;

        // Pawner (Customer) Details
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }

        // Owner Details (If different from Pawner - Form-E Requirement)
        public string? OwnerName { get; set; }
        public string? OwnerAddress { get; set; }

        // Article Details
        public string ArticleDescription { get; set; } = string.Empty;
        public decimal GrossWeightGrams { get; set; }
        public decimal NetWeightGrams { get; set; }
        public decimal PresentValue { get; set; }

        // Loan Financials
        public decimal PrincipalAmount { get; set; }
        public decimal MonthlyInterestRate { get; set; }

        // Lifecycle: "PENDING", "REDEEMED", "AUCTIONED"
        public string Status { get; set; } = "PENDING";
        
        // Redemption & Auction Tracking
        public DateTime? RedemptionDate { get; set; }
        public string? RedeemerName { get; set; }
        public string? RedeemerAddress { get; set; }
        public DateTime? AuctionDate { get; set; }

        // Collection of partial interest/principal payments
        public List<PledgePayment> Payments { get; set; } = new();
    }

    public class PledgePayment
    {
        public int PledgePaymentId { get; set; }
        public int PledgeId { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public decimal AmountPaid { get; set; }
        public decimal InterestComponent { get; set; }
        public decimal PrincipalComponent { get; set; }
    }
}