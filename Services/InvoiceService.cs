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

    public async Task<List<Customer>> GetCustomersAsync()
    {
        return await _context.Customers
            .AsNoTracking()
            .OrderBy(customer => customer.Name)
            .ToListAsync();
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        return await _context.Products
            .AsNoTracking()
            .OrderBy(product => product.Name)
            .ToListAsync();
    }

    public async Task<Invoice> CreateInvoiceAsync(Invoice invoice)
    {
        if (invoice.Items.Count == 0)
        {
            throw new InvalidOperationException("An invoice must contain at least one item.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        foreach (var item in invoice.Items)
        {
            var product = await _context.Products.FirstOrDefaultAsync(product => product.Id == item.ProductId);

            if (product is null)
            {
                throw new InvalidOperationException($"Product with Id {item.ProductId} was not found.");
            }

            if (item.Quantity <= 0)
            {
                throw new InvalidOperationException("Quantity must be greater than zero.");
            }

            if (product.StockQuantity < item.Quantity)
            {
                throw new InvalidOperationException($"Not enough stock for {product.Name}.");
            }

            item.UnitPrice = product.Price;
            item.LineTotal = item.Quantity * item.UnitPrice;
            product.StockQuantity -= item.Quantity;
        }

        invoice.TotalAmount = invoice.Items.Sum(item => item.LineTotal);

        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return invoice;
    }

    public async Task<List<Invoice>> GetAllInvoicesAsync()
    {
        return await _context.Invoices
            .AsNoTracking()
            .Include(invoice => invoice.Customer)
            .Include(invoice => invoice.Items)
            .ThenInclude(item => item.Product)
            .OrderByDescending(invoice => invoice.InvoiceDate)
            .ToListAsync();
    }

    public async Task<List<Invoice>> SearchInvoicesAsync(string searchTerm)
    {
        searchTerm = searchTerm.Trim();

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetAllInvoicesAsync();
        }

        var query = _context.Invoices
            .AsNoTracking()
            .Include(invoice => invoice.Customer)
            .Include(invoice => invoice.Items)
            .ThenInclude(item => item.Product)
            .AsQueryable();

        if (int.TryParse(searchTerm, out var invoiceId))
        {
            query = query.Where(invoice => invoice.Id == invoiceId);
        }
        else if (DateTime.TryParse(searchTerm, out var invoiceDate))
        {
            query = query.Where(invoice => invoice.InvoiceDate.Date == invoiceDate.Date);
        }
        else
        {
            query = query.Where(invoice =>
                invoice.Customer != null &&
                (
                    invoice.Customer.Name.Contains(searchTerm) ||
                    (invoice.Customer.PhoneNumber != null && invoice.Customer.PhoneNumber.Contains(searchTerm))
                ));
        }

        return await query
            .OrderByDescending(invoice => invoice.InvoiceDate)
            .ToListAsync();
    }

    public async Task<Invoice?> GetInvoiceByIdAsync(int id)
    {
        return await _context.Invoices
            .Include(invoice => invoice.Customer)
            .Include(invoice => invoice.Items)
            .ThenInclude(item => item.Product)
            .FirstOrDefaultAsync(invoice => invoice.Id == id);
    }

    public async Task DeleteInvoiceAsync(int id)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        var invoice = await _context.Invoices
            .Include(invoice => invoice.Items)
            .FirstOrDefaultAsync(invoice => invoice.Id == id);

        if (invoice is null)
        {
            throw new InvalidOperationException($"Invoice with Id {id} was not found.");
        }

        foreach (var item in invoice.Items)
        {
            var product = await _context.Products.FirstOrDefaultAsync(product => product.Id == item.ProductId);

            if (product is not null)
            {
                product.StockQuantity += item.Quantity;
            }
        }

        _context.Invoices.Remove(invoice);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}
