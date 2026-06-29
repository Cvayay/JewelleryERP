namespace JewelleryERP.Helpers;

public sealed class DashboardSummary
{
    public int TotalCustomers { get; init; }

    public int TotalProducts { get; init; }

    public int TotalInvoices { get; init; }

    public decimal TodaysSalesAmount { get; init; }
}
