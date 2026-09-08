using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JewelleryERP.Models;
using JewelleryERP.Services;

namespace JewelleryERP.ViewModels;

public partial class CustomerLedgerViewModel : ObservableObject
{
    private readonly CustomerService _customerService;
    private readonly InvoiceService _invoiceService;
    private readonly LoanService _loanService;

    // ── Collections ───────────────────────────────────────────────────────

    [ObservableProperty] private ObservableCollection<Customer> customers = new();
    [ObservableProperty] private Customer? selectedCustomer;
    [ObservableProperty] private ObservableCollection<LedgerTransaction> ledgerTransactions = new();

    // ── Summary Statistics ────────────────────────────────────────────────

    [ObservableProperty] private string statusMessage = string.Empty;
    [ObservableProperty] private string customerName = string.Empty;
    [ObservableProperty] private string customerPhone = string.Empty;
    [ObservableProperty] private string customerAddress = string.Empty;

    [ObservableProperty] private decimal totalDebit = 0m;
    [ObservableProperty] private decimal totalCredit = 0m;
    [ObservableProperty] private decimal closingBalance = 0m;

    // ── Commands ──────────────────────────────────────────────────────────

    public IAsyncRelayCommand LoadCustomersCommand { get; }
    public IAsyncRelayCommand<Customer?> LoadLedgerCommand { get; }

    public CustomerLedgerViewModel(
        CustomerService customerService,
        InvoiceService invoiceService,
        LoanService loanService)
    {
        _customerService = customerService;
        _invoiceService = invoiceService;
        _loanService = loanService;

        LoadCustomersCommand = new AsyncRelayCommand(LoadCustomersAsync);
        LoadLedgerCommand = new AsyncRelayCommand<Customer?>(LoadLedgerAsync);

        // Auto-load ledger when customer selection changes
        PropertyChanging += (s, e) =>
        {
            if (e.PropertyName == nameof(SelectedCustomer) && SelectedCustomer != null)
            {
                _ = LoadLedgerAsync(SelectedCustomer);
            }
        };
    }

    // ── Methods ───────────────────────────────────────────────────────────

    private async Task LoadCustomersAsync()
    {
        try
        {
            StatusMessage = "Loading customers...";
            var customersList = await _customerService.GetAllCustomersAsync();
            Customers = new ObservableCollection<Customer>(customersList);
            StatusMessage = $"Loaded {customersList.Count} customer(s).";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    private async Task LoadLedgerAsync(Customer? customer)
    {
        if (customer is null)
        {
            LedgerTransactions.Clear();
            CustomerName = string.Empty;
            CustomerPhone = string.Empty;
            CustomerAddress = string.Empty;
            TotalDebit = 0m;
            TotalCredit = 0m;
            ClosingBalance = 0m;
            return;
        }

        try
        {
            StatusMessage = "Loading ledger...";
            CustomerName = customer.Name;
            CustomerPhone = customer.PhoneNumber ?? "N/A";
            CustomerAddress = customer.Address ?? "N/A";

            // Fetch all invoices for this customer
            var allInvoices = await _invoiceService.GetAllInvoicesAsync();
            var customerInvoices = allInvoices
                .Where(i => i.CustomerId == customer.Id)
                .OrderBy(i => i.InvoiceDate)
                .ToList();

            // Fetch all loans for this customer
            var allLoans = await _loanService.GetAllLoansAsync();
            var customerLoans = allLoans
                .Where(l => l.CustomerId == customer.Id)
                .OrderBy(l => l.LoanDate)
                .ToList();

            // Build ledger transactions
            var transactions = new List<LedgerTransaction>();

            // Add invoice entries (DEBIT - customer owes us)
            foreach (var invoice in customerInvoices)
            {
                transactions.Add(new LedgerTransaction
                {
                    TransactionDate = invoice.InvoiceDate,
                    TransactionType = "Invoice",
                    ReferenceNumber = invoice.BillNumber,
                    Description = $"Bill #{invoice.BillNumber} - Gold Jewellery",
                    DebitAmount = invoice.TotalAmount,
                    CreditAmount = 0m
                });
            }

            // Add loan entries (CREDIT - we received items from customer)
            foreach (var loan in customerLoans)
            {
                var description = $"Loan #{loan.LoanNumber}";
                if (!string.IsNullOrEmpty(loan.ArticleDescription))
                {
                    description += $" - {loan.ArticleDescription}";
                }

                transactions.Add(new LedgerTransaction
                {
                    TransactionDate = loan.LoanDate,
                    TransactionType = "Loan",
                    ReferenceNumber = loan.LoanNumber,
                    Description = description,
                    DebitAmount = 0m,
                    CreditAmount = loan.PrincipalAmount
                });

                // Add redemption if status is Redeemed
                if (loan.Status == LoanStatus.Redeemed && loan.RedemptionDate.HasValue)
                {
                    // Calculate interest accrued
                    var daysHeld = (loan.RedemptionDate.Value.Date - loan.LoanDate.Date).Days;
                    var interest = loan.PrincipalAmount * (loan.InterestRate / 100m) * (daysHeld / 30m);

                    transactions.Add(new LedgerTransaction
                    {
                        TransactionDate = loan.RedemptionDate.Value,
                        TransactionType = "Redemption",
                        ReferenceNumber = loan.LoanNumber,
                        Description = $"Redemption of Loan #{loan.LoanNumber} (Interest: ₹{interest:N2})",
                        DebitAmount = loan.PrincipalAmount + interest,
                        CreditAmount = 0m
                    });
                }
            }

            // Sort by date
            transactions = transactions.OrderBy(t => t.TransactionDate).ToList();

            // Calculate running balances and statistics
            decimal runningBalance = 0m;
            decimal totalDeb = 0m;
            decimal totalCred = 0m;

            foreach (var transaction in transactions)
            {
                runningBalance += transaction.DebitAmount - transaction.CreditAmount;
                transaction.RunningBalance = runningBalance;
                totalDeb += transaction.DebitAmount;
                totalCred += transaction.CreditAmount;
            }

            LedgerTransactions = new ObservableCollection<LedgerTransaction>(transactions);
            TotalDebit = totalDeb;
            TotalCredit = totalCred;
            ClosingBalance = runningBalance;

            StatusMessage = $"Loaded {transactions.Count} transaction(s) for {customer.Name}.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }
}

/// <summary>
/// Represents a single ledger entry combining invoices and loans.
/// </summary>
public class LedgerTransaction
{
    public DateTime TransactionDate { get; set; }
    public string TransactionType { get; set; } = string.Empty; // Invoice, Loan, Redemption, Auction
    public string ReferenceNumber { get; set; } = string.Empty; // Bill number or Loan number
    public string Description { get; set; } = string.Empty;
    public decimal DebitAmount { get; set; } // Customer owes us (invoices, redemptions)
    public decimal CreditAmount { get; set; } // We owe customer (loans received)
    public decimal RunningBalance { get; set; } // Cumulative: positive = customer owes, negative = we owe
}
