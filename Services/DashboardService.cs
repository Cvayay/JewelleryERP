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

        var totalCustomersTask = _context.Customers.AsNoTracking().CountAsync();
        var totalProductsTask = _context.Products.AsNoTracking().CountAsync();
        var totalInvoicesTask = _context.Invoices.AsNoTracking().CountAsync();
        var todaysSalesTask = _context.Invoices
            .AsNoTracking()
            .Where(invoice => invoice.InvoiceDate >= today && invoice.InvoiceDate < tomorrow)
            .Select(invoice => (decimal?)invoice.TotalAmount)
            .SumAsync();

        await Task.WhenAll(totalCustomersTask, totalProductsTask, totalInvoicesTask, todaysSalesTask);

        return new DashboardSummary
        {
            TotalCustomers = await totalCustomersTask,
            TotalProducts = await totalProductsTask,
            TotalInvoices = await totalInvoicesTask,
            TodaysSalesAmount = await todaysSalesTask ?? 0m
        };
    }
}
