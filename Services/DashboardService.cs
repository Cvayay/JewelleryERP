using JewelleryERP.Data;
using JewelleryERP.Helpers;
using JewelleryERP.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JewelleryERP.Services;

public class DashboardService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public DashboardService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<DashboardSummary> GetSummaryAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var customers = await context.Customers.AsNoTracking().CountAsync();
            var products = await context.Products.AsNoTracking().ToListAsync();
            var invoices = await context.Invoices
                .AsNoTracking()
                .Include(invoice => invoice.Customer)
                .OrderByDescending(invoice => invoice.InvoiceDate)
                .ToListAsync();
            var loans = await context.Loans
                .AsNoTracking()
                .Include(loan => loan.Customer)
                .OrderByDescending(loan => loan.LoanDate)
                .ToListAsync();

            var todaysInvoices = invoices
                .Where(invoice => invoice.InvoiceDate >= today && invoice.InvoiceDate < tomorrow)
                .ToList();
            var activeLoans = loans.Where(loan => loan.Status == LoanStatus.Pending).ToList();
            var alerts = activeLoans
                .Where(loan => loan.IsOverdue6Months)
                .Select(loan => $"Pledge {loan.LoanNumber} has been pending for {loan.MonthsElapsed} month(s).")
                .ToList();

            return new DashboardSummary
            {
                TotalCustomers = customers,
                TotalProducts = products.Count,
                TotalInvoices = invoices.Count,
                TodaysSalesAmount = todaysInvoices.Sum(invoice => invoice.TotalAmount),
                TodaysCgstAmount = todaysInvoices.Sum(invoice => invoice.CgstAmount),
                TodaysSgstAmount = todaysInvoices.Sum(invoice => invoice.SgstAmount),
                InventoryValue = products.Sum(product => product.Quantity * product.SellingPrice),
                ActivePledgeCount = activeLoans.Count,
                LoanExposure = activeLoans.Sum(loan => loan.PrincipalAmount),
                GoldStockGrams = products
                    .Where(product => product.MetalType.Equals("Gold", StringComparison.OrdinalIgnoreCase))
                    .Sum(product => product.NetWeight * product.Quantity),
                SilverStockGrams = products
                    .Where(product => product.MetalType.Equals("Silver", StringComparison.OrdinalIgnoreCase))
                    .Sum(product => product.NetWeight * product.Quantity),
                RecentInvoices = invoices.Take(8).Select(invoice => new DashboardInvoiceRow
                {
                    BillNumber = invoice.BillNumber,
                    CustomerName = invoice.Customer?.Name ?? "Walk-in customer",
                    InvoiceDate = invoice.InvoiceDate,
                    TotalAmount = invoice.TotalAmount
                }).ToList(),
                RecentPledges = loans.Take(8).Select(loan => new DashboardLoanRow
                {
                    LoanNumber = loan.LoanNumber,
                    CustomerName = loan.Customer?.Name ?? "Unknown customer",
                    LoanDate = loan.LoanDate,
                    PrincipalAmount = loan.PrincipalAmount,
                    Status = loan.Status
                }).ToList(),
                Alerts = alerts
            };
        }
        catch (Exception ex)
        {
            AppPaths.WriteError(nameof(DashboardService), ex);
            throw;
        }
    }
}
