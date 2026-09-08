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
                product.ProductCode.Contains(searchTerm) ||
                (product.Barcode != null && product.Barcode.Contains(searchTerm)) ||
                (product.Category != null && product.Category.Contains(searchTerm)))
            .OrderBy(product => product.Name)
            .ToListAsync();
    }

    public async Task<Product> SaveProductAsync(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        if (string.IsNullOrWhiteSpace(product.Name))
            throw new InvalidOperationException("Product name is required.");
        
        if (string.IsNullOrWhiteSpace(product.ProductCode))
            throw new InvalidOperationException("Product code is required.");

        if (product.SellingPrice < 0)
            throw new InvalidOperationException("Selling price cannot be negative.");

        if (product.Quantity < 0)
            throw new InvalidOperationException("Quantity cannot be negative.");

        product.Name = product.Name.Trim();
        product.ProductCode = product.ProductCode.Trim();
        product.Category = string.IsNullOrWhiteSpace(product.Category) ? string.Empty : product.Category.Trim();

        if (product.Id == 0)
        {
            product.CreatedAt = DateTime.UtcNow;
            _context.Products.Add(product);
        }
        else
        {
            var existingProduct = await _context.Products.FirstOrDefaultAsync(item => item.Id == product.Id);

            if (existingProduct is null)
                throw new InvalidOperationException($"Product with Id {product.Id} was not found.");

            existingProduct.ProductCode = product.ProductCode;
            existingProduct.Barcode = product.Barcode;
            existingProduct.Name = product.Name;
            existingProduct.Category = product.Category;
            existingProduct.MetalType = product.MetalType;
            existingProduct.Purity = product.Purity;
            existingProduct.Weight = product.Weight;
            existingProduct.GrossWeight = product.GrossWeight;
            existingProduct.StoneWeight = product.StoneWeight;
            existingProduct.NetWeight = product.NetWeight;
            existingProduct.MetalRate = product.MetalRate;
            existingProduct.MakingCharge = product.MakingCharge;
            existingProduct.MakingChargeType = product.MakingChargeType;
            existingProduct.StoneCost = product.StoneCost;
            existingProduct.SellingPrice = product.SellingPrice;
            existingProduct.Quantity = product.Quantity;
        }

        await _context.SaveChangesAsync();
        return product;
    }

    public async Task DeleteProductAsync(int id)
    {
        var product = await _context.Products.FirstOrDefaultAsync(item => item.Id == id);
        if (product is null) return;

        var isUsedInInvoice = await _context.InvoiceItems.AnyAsync(item => item.ProductId == id);
        if (isUsedInInvoice)
        {
            throw new InvalidOperationException("This product is used in invoices and cannot be deleted. Set quantity to 0 instead.");
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
    }
}