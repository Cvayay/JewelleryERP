using JewelleryERP.Data;
using JewelleryERP.Models;
using Microsoft.EntityFrameworkCore;

namespace JewelleryERP.Services;

public class ProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {
        return await _context.Products
            .AsNoTracking()
            .OrderBy(product => product.Name)
            .ToListAsync();
    }

    public async Task<List<Product>> SearchProductsAsync(string searchTerm)
    {
        searchTerm = searchTerm.Trim();

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetAllProductsAsync();
        }

        return await _context.Products
            .AsNoTracking()
            .Where(product =>
                product.Name.Contains(searchTerm) ||
                (product.Category != null && product.Category.Contains(searchTerm)))
            .OrderBy(product => product.Name)
            .ToListAsync();
    }

    public async Task<Product> SaveProductAsync(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        if (string.IsNullOrWhiteSpace(product.Name))
        {
            throw new InvalidOperationException("Product name is required.");
        }

        if (product.Price < 0)
        {
            throw new InvalidOperationException("Price cannot be negative.");
        }

        if (product.StockQuantity < 0)
        {
            throw new InvalidOperationException("Stock cannot be negative.");
        }

        if (product.Id == 0)
        {
            product.Name = product.Name.Trim();
            product.Category = string.IsNullOrWhiteSpace(product.Category) ? null : product.Category.Trim();
            product.CreatedAt = DateTime.Now;
            _context.Products.Add(product);
        }
        else
        {
            var existingProduct = await _context.Products.FirstOrDefaultAsync(item => item.Id == product.Id);

            if (existingProduct is null)
            {
                throw new InvalidOperationException($"Product with Id {product.Id} was not found.");
            }

            existingProduct.Name = product.Name.Trim();
            existingProduct.Category = string.IsNullOrWhiteSpace(product.Category) ? null : product.Category.Trim();
            existingProduct.Price = product.Price;
            existingProduct.StockQuantity = product.StockQuantity;
        }

        await _context.SaveChangesAsync();
        return product;
    }

    public async Task DeleteProductAsync(int id)
    {
        var product = await _context.Products.FirstOrDefaultAsync(item => item.Id == id);

        if (product is null)
        {
            return;
        }

        var isUsedInInvoice = await _context.InvoiceItems.AnyAsync(item => item.ProductId == id);

        if (isUsedInInvoice)
        {
            throw new InvalidOperationException("This product is used in invoices and cannot be deleted. Set stock to 0 instead.");
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
    }
}
