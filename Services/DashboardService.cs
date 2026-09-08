using JewelleryERP.Data;
using JewelleryERP.Helpers;
using Microsoft.EntityFrameworkCore;

namespace JewelleryERP.Services;

public class DashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummary> GetSummaryAsync()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        var totalCustomers = await _context.Customers.AsNoTracking().CountAsync();
        var totalProducts = await _context.Products.AsNoTracking().CountAsync();
        var totalInvoices = await _context.Invoices.AsNoTracking().CountAsync();
        var todaysSalesAmount = await _context.Invoices
            .AsNoTracking()
            .Where(invoice => invoice.InvoiceDate >= today && invoice.InvoiceDate < tomorrow)
            .Select(invoice => (decimal?)invoice.TotalAmount)
            .SumAsync();

        return new DashboardSummary
        {
            TotalCustomers = totalCustomers,
            TotalProducts = totalProducts,
            TotalInvoices = totalInvoices,
            TodaysSalesAmount = todaysSalesAmount ?? 0m
        };
    }
}
