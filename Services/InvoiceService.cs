using JewelleryERP.Data;
using JewelleryERP.Models;
using Microsoft.EntityFrameworkCore;

namespace JewelleryERP.Services;

public class InvoiceService
{
    private readonly AppDbContext _context;

    public InvoiceService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Invoice>> GetInvoicesAsync()
    {
        return await _context.Invoices
            .Include(i => i.Customer)
            .Include(i => i.Items)
                .ThenInclude(item => item.Product)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    public async Task AddInvoiceAsync(Invoice invoice)
    {
        // 1. Calculate Line Totals and Adjust Inventory Stock
        foreach (var item in invoice.Items)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product != null)
            {
                // Reduce stock by 1 for sold items (or adjust based on business logic)
                product.StockQuantity -= 1;
            }

            // Calculation formula: (NetWeight * RatePerGram) + MakingCharges
            item.LineTotal = (item.NetWeight * item.RatePerGram) + item.MakingCharges;
        }

        // 2. Compute Invoice Financial Summary
        invoice.SubTotal = invoice.Items.Sum(item => item.NetWeight * item.RatePerGram);
        invoice.MakingChargesTotal = invoice.Items.Sum(item => item.MakingCharges);
        
        var taxableValue = invoice.SubTotal + invoice.MakingChargesTotal;
        invoice.CgstAmount = Math.Round(taxableValue * 0.015m, 2); // 1.50% CGST
        invoice.SgstAmount = Math.Round(taxableValue * 0.015m, 2); // 1.50% SGST
        invoice.GrandTotal = taxableValue + invoice.CgstAmount + invoice.SgstAmount;

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteInvoiceAsync(int invoiceId)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

        if (invoice != null)
        {
            // Restore inventory stock
            foreach (var item in invoice.Items)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity += 1;
                }
            }

            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();
        }
    }
}