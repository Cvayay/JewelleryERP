using Microsoft.EntityFrameworkCore;
using JewelleryERP.Data;
using JewelleryERP.Models;

namespace JewelleryERP.Services;

public class LoanService
{
    private readonly AppDbContext _context;

    public LoanService(AppDbContext context) => _context = context;

    // ── Create ────────────────────────────────────────────────────────────

    public async Task AddLoanAsync(Loan loan)
    {
        loan.CreatedAt = DateTime.Now;

        // Auto-generate loan number if not provided
        if (string.IsNullOrWhiteSpace(loan.LoanNumber))
        {
            var last = await _context.Loans
                .OrderByDescending(l => l.Id)
                .Select(l => l.Id)
                .FirstOrDefaultAsync();
            loan.LoanNumber = $"L{(last + 1):D5}";
        }

        await _context.Loans.AddAsync(loan);
        await _context.SaveChangesAsync();
    }

    // ── Read ──────────────────────────────────────────────────────────────

    public async Task<List<Loan>> GetAllLoansAsync()
    {
        return await _context.Loans
            .Include(l => l.Customer)
            .AsNoTracking()
            .OrderByDescending(l => l.LoanDate)
            .ToListAsync();
    }

    public async Task<List<Loan>> GetPendingLoansAsync()
    {
        return await _context.Loans
            .Include(l => l.Customer)
            .AsNoTracking()
            .Where(l => l.Status == LoanStatus.Pending)
            .OrderBy(l => l.LoanDate)
            .ToListAsync();
    }

    /// <summary>Returns pending loans where loan date is older than <paramref name="months"/> months.</summary>
    public async Task<List<Loan>> GetOverdueLoansAsync(int months = 6)
    {
        var cutoff = DateTime.Now.AddMonths(-months);
        return await _context.Loans
            .Include(l => l.Customer)
            .AsNoTracking()
            .Where(l => l.Status == LoanStatus.Pending && l.LoanDate <= cutoff)
            .OrderBy(l => l.LoanDate)
            .ToListAsync();
    }

    public async Task<List<Loan>> SearchLoansAsync(string searchText)
    {
        searchText = searchText?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(searchText))
            return await GetAllLoansAsync();

        return await _context.Loans
            .Include(l => l.Customer)
            .AsNoTracking()
            .Where(l =>
                EF.Functions.Like(l.LoanNumber, $"%{searchText}%") ||
                (l.Customer != null && EF.Functions.Like(l.Customer.Name, $"%{searchText}%")) ||
                (l.Customer != null && EF.Functions.Like(l.Customer.PhoneNumber, $"%{searchText}%")))
            .OrderByDescending(l => l.LoanDate)
            .ToListAsync();
    }

    // ── Lifecycle ─────────────────────────────────────────────────────────

    public async Task RedeemLoanAsync(int loanId, string redeemerName, string redeemerAddress)
    {
        var loan = await _context.Loans.FindAsync(loanId)
            ?? throw new InvalidOperationException($"Loan #{loanId} not found.");

        if (loan.Status != LoanStatus.Pending)
            throw new InvalidOperationException("Only a Pending loan can be redeemed.");

        if (string.IsNullOrWhiteSpace(redeemerName))
            throw new ArgumentException("Redeemer name is required.");

        loan.Status = LoanStatus.Redeemed;
        loan.RedemptionDate = DateTime.Now;
        loan.RedeemerName = redeemerName.Trim();
        loan.RedeemerAddress = redeemerAddress?.Trim();

        await _context.SaveChangesAsync();
    }

    public async Task AuctionLoanAsync(int loanId)
    {
        var loan = await _context.Loans.FindAsync(loanId)
            ?? throw new InvalidOperationException($"Loan #{loanId} not found.");

        if (loan.Status != LoanStatus.Pending)
            throw new InvalidOperationException("Only a Pending loan can be auctioned.");

        loan.Status = LoanStatus.Auctioned;
        loan.AuctionDate = DateTime.Now;

        await _context.SaveChangesAsync();
    }

    // ── Interest ──────────────────────────────────────────────────────────

    /// <summary>
    /// Simple interest: Principal × (Rate / 100) × Months.
    /// Rate is monthly percentage (e.g. 2.0 = 2% per month).
    /// </summary>
    public static decimal CalculateInterest(Loan loan, DateTime? asOf = null)
    {
        var referenceDate = asOf ?? DateTime.Now;
        var months = (int)Math.Floor((referenceDate - loan.LoanDate).TotalDays / 30.0);
        months = Math.Max(months, 0);
        return loan.PrincipalAmount * loan.InterestRate / 100m * months;
    }

    public static decimal CalculateTotalDue(Loan loan, DateTime? asOf = null)
        => loan.PrincipalAmount + CalculateInterest(loan, asOf);
}