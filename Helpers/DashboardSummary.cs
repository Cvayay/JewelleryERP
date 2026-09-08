using JewelleryERP.Models;

namespace JewelleryERP.Helpers;

public sealed class DashboardSummary
{
    public int TotalCustomers { get; init; }

    public int TotalProducts { get; init; }

    public int TotalInvoices { get; init; }

    public decimal TodaysSalesAmount { get; init; }

    public decimal TodaysCgstAmount { get; init; }

    public decimal TodaysSgstAmount { get; init; }

    public decimal InventoryValue { get; init; }

    public int ActivePledgeCount { get; init; }

    public decimal LoanExposure { get; init; }

    public decimal GoldStockGrams { get; init; }

    public decimal SilverStockGrams { get; init; }

    public IReadOnlyList<DashboardInvoiceRow> RecentInvoices { get; init; } = [];

    public IReadOnlyList<DashboardLoanRow> RecentPledges { get; init; } = [];

    public IReadOnlyList<string> Alerts { get; init; } = [];
}

public sealed class DashboardInvoiceRow
{
    public string BillNumber { get; init; } = string.Empty;

    public string CustomerName { get; init; } = string.Empty;

    public DateTime InvoiceDate { get; init; }

    public decimal TotalAmount { get; init; }
}

public sealed class DashboardLoanRow
{
    public string LoanNumber { get; init; } = string.Empty;

    public string CustomerName { get; init; } = string.Empty;

    public DateTime LoanDate { get; init; }

    public decimal PrincipalAmount { get; init; }

    public LoanStatus Status { get; init; }
}
