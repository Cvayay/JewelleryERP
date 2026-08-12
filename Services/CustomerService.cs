using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JewelleryERP.Data;
using JewelleryERP.Models;
using Microsoft.EntityFrameworkCore;

namespace JewelleryERP.Services;

public class CustomerService
{
    private readonly AppDbContext _context;

    public CustomerService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Customer>> GetAllCustomersAsync()
    {
        return await _context.Customers.AsNoTracking().ToListAsync();
    }

    public async Task<List<Customer>> SearchCustomersAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return await GetAllCustomersAsync();

        var q = query.ToLower();
        return await _context.Customers
            .AsNoTracking()
            .Where(c => c.Name.ToLower().Contains(q) || 
                        (c.PhoneNumber != null && c.PhoneNumber.Contains(q)) ||
                        (c.GstNumber != null && c.GstNumber.ToLower().Contains(q)) ||
                        (c.Email != null && c.Email.ToLower().Contains(q)))
            .ToListAsync();
    }

    public async Task AddCustomerAsync(Customer customer)
    {
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateCustomerAsync(Customer customer)
    {
        _context.Customers.Update(customer);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCustomerAsync(int customerId)
    {
        var customer = await _context.Customers.FindAsync(customerId);
        if (customer != null)
        {
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
        }
    }
}