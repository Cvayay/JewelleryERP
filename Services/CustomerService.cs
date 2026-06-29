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

    public async Task AddCustomerAsync(Customer customer)
    {
        if (customer.CreatedAt == default)
        {
            customer.CreatedAt = DateTime.Now;
        }

        await _context.Customers.AddAsync(customer);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Customer>> GetAllCustomersAsync()
    {
        return await _context.Customers
            .AsNoTracking()
            .OrderBy(customer => customer.Name)
            .ToListAsync();
    }

    public async Task<List<Customer>> SearchCustomersAsync(string searchTerm)
    {
        searchTerm = searchTerm.Trim();

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return await GetAllCustomersAsync();
        }

        return await _context.Customers
            .AsNoTracking()
            .Where(customer =>
                customer.Name.Contains(searchTerm) ||
                (customer.PhoneNumber != null && customer.PhoneNumber.Contains(searchTerm)) ||
                (customer.Address != null && customer.Address.Contains(searchTerm)))
            .OrderBy(customer => customer.Name)
            .ToListAsync();
    }

    public async Task UpdateCustomerAsync(Customer customer)
    {
        var existingCustomer = await _context.Customers
            .FirstOrDefaultAsync(item => item.Id == customer.Id);

        if (existingCustomer is null)
        {
            throw new InvalidOperationException($"Customer with Id {customer.Id} was not found.");
        }

        existingCustomer.Name = customer.Name;
        existingCustomer.PhoneNumber = customer.PhoneNumber;
        existingCustomer.Address = customer.Address;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteCustomerAsync(int id)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(item => item.Id == id);

        if (customer is null)
        {
            throw new InvalidOperationException($"Customer with Id {id} was not found.");
        }

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
    }
}
