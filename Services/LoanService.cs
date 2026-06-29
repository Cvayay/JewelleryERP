using Microsoft.EntityFrameworkCore;
using JewelleryERP.Data;
using JewelleryERP.Models;

namespace JewelleryERP.Services;

public class LoanService
{
    private readonly AppDbContext _context;

    public LoanService(AppDbContext context)
    {
        _context = context;
    }

    // 1. Add Loan
    public async Task AddLoanAsync(Loan loan)
    {
        await _context.Loans.AddAsync(loan);
        await _context.SaveChangesAsync();
    }

    // 2. Get All Loans
    public async Task<List<Loan>> GetAllLoansAsync()
    {
        return await _context.Loans
            .Include(l => l.Customer)
            .AsNoTracking()
            .ToListAsync();
    }

    // 3. Search Loans
    public async Task<List<Loan>> SearchLoansAsync(string searchText)
    {
        searchText = searchText?.ToLower() ?? "";

return await _context.Loans
    .Include(l => l.Customer)
    .AsNoTracking()
    .Where(l =>
        EF.Functions.Like(l.LoanNumber, $"%{searchText}%") ||
        (l.Customer != null && EF.Functions.Like(l.Customer.Name, $"%{searchText}%")) ||
        (l.Customer != null && EF.Functions.Like(l.Customer.PhoneNumber, $"%{searchText}%"))
    )
    .ToListAsync();
    }

    // 4. Redeem Loan
    public async Task RedeemLoanAsync(int loanId, string redeemerName, string redeemerAddress)
    {
        var loan = await _context.Loans.FirstOrDefaultAsync(l => l.Id == loanId);

        if (loan == null)
            throw new Exception("Loan not found");

        loan.Status = LoanStatus.Redeemed;
        loan.RedemptionDate = DateTime.Now;
        loan.RedeemerName = redeemerName;
        loan.RedeemerAddress = redeemerAddress;

        await _context.SaveChangesAsync();
    }

    // 5. Auction Loan
    public async Task AuctionLoanAsync(int loanId)
    {
        var loan = await _context.Loans.FirstOrDefaultAsync(l => l.Id == loanId);

        if (loan == null)
            throw new Exception("Loan not found");

        loan.Status = LoanStatus.Auctioned;

        await _context.SaveChangesAsync();
    }
}