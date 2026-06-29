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
}
