using JewelleryERP.Data;
using Microsoft.EntityFrameworkCore;

namespace JewelleryERP.Services;

public class DashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(decimal TotalSales, int TotalCustomers, int TotalProducts, decimal TotalPendingLoans)> GetDashboardMetricsAsync()
    {
        var totalSales = await _context.Invoices.SumAsync(i => i.GrandTotal);
        var totalCustomers = await _context.Customers.CountAsync();
        var totalProducts = await _context.Products.CountAsync();
        var totalPendingLoans = await _context.Loans
            .Where(l => l.Status == Models.LoanStatus.Pending)
            .SumAsync(l => l.PrincipalAmount);

        return (totalSales, totalCustomers, totalProducts, totalPendingLoans);
    }
}