using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JewelleryERP.Models;
using JewelleryERP.Services;

namespace JewelleryERP.ViewModels;

public partial class LoanViewModel : ObservableObject
{
    private readonly LoanService _loanService;
    private readonly CustomerService _customerService;
    private readonly SettingService _settingService;

    // ── Lists ─────────────────────────────────────────────────────────────

    [ObservableProperty] private ObservableCollection<Loan> loans = new();
    [ObservableProperty] private ObservableCollection<Customer> customers = new();
    [ObservableProperty] private Loan? selectedLoan;

    // ── New Loan Form ─────────────────────────────────────────────────────

    [ObservableProperty] private Customer? selectedCustomer;
    [ObservableProperty] private DateTime loanDate = DateTime.Today;
    [ObservableProperty] private decimal principalAmount;
    [ObservableProperty] private decimal interestRate;
    [ObservableProperty] private decimal weight;
    [ObservableProperty] private decimal presentValue;
    [ObservableProperty] private string articleDescription = string.Empty;
    [ObservableProperty] private string ownerName = string.Empty;
    [ObservableProperty] private string ownerAddress = string.Empty;

    // ── Redeem panel ──────────────────────────────────────────────────────
    [ObservableProperty] private string redeemerName = string.Empty;
    [ObservableProperty] private string redeemerAddress = string.Empty;

    // ── Filter / Search ───────────────────────────────────────────────────
    [ObservableProperty] private string searchText = string.Empty;
    [ObservableProperty] private string filterStatus = "All"; // All | Pending | Redeemed | Auctioned

    // ── Status / Computed display ─────────────────────────────────────────
    [ObservableProperty] private string statusMessage = string.Empty;
    [ObservableProperty] private string selectedLoanInterest = string.Empty;
    [ObservableProperty] private string selectedLoanTotalDue = string.Empty;

    // ── Commands ──────────────────────────────────────────────────────────
    public IAsyncRelayCommand LoadLoansCommand { get; }
    public IAsyncRelayCommand AddLoanCommand { get; }
    public IAsyncRelayCommand RedeemLoanCommand { get; }
    public IAsyncRelayCommand AuctionLoanCommand { get; }
    public IAsyncRelayCommand SearchCommand { get; }
    public IRelayCommand ClearFormCommand { get; }

    public LoanViewModel(
        LoanService loanService,
        CustomerService customerService,
        SettingService settingService)
    {
        _loanService = loanService;
        _customerService = customerService;
        _settingService = settingService;

        LoadLoansCommand = new AsyncRelayCommand(LoadLoansAsync);
        AddLoanCommand = new AsyncRelayCommand(AddLoanAsync);
        RedeemLoanCommand = new AsyncRelayCommand(RedeemLoanAsync);
        AuctionLoanCommand = new AsyncRelayCommand(AuctionLoanAsync);
        SearchCommand = new AsyncRelayCommand(SearchAsync);
        ClearFormCommand = new RelayCommand(ClearForm);
    }

    // ── Methods ───────────────────────────────────────────────────────────

    private async Task LoadLoansAsync()
    {
        try
        {
            StatusMessage = "Loading loans...";
            var allLoans = await _loanService.GetAllLoansAsync();
            Loans = new ObservableCollection<Loan>(allLoans);
            StatusMessage = $"Loaded {allLoans.Count} loan(s).";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading loans: {ex.Message}";
        }
    }

    private async Task AddLoanAsync()
    {
        if (SelectedCustomer is null || PrincipalAmount <= 0)
        {
            StatusMessage = "Select a customer and enter a valid principal amount.";
            return;
        }

        try
        {
            var loan = new Loan
            {
                LoanNumber = $"LOAN-{DateTime.Now:yyyyMMddHHmmss}",
                CustomerId = SelectedCustomer.Id,
                LoanDate = LoanDate,
                PrincipalAmount = PrincipalAmount,
                InterestRate = InterestRate > 0 ? InterestRate : (await _settingService.GetSettingsAsync())?.DefaultInterestRate ?? 2m,
                Weight = Weight,
                PresentValue = PresentValue,
                ArticleDescription = ArticleDescription,
                OwnerName = OwnerName,
                OwnerAddress = OwnerAddress,
                Status = LoanStatus.Pending,
                CreatedAt = DateTime.Now
            };

            await _loanService.AddLoanAsync(loan);
            StatusMessage = "Loan created successfully.";
            ClearForm();
            await LoadLoansAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error creating loan: {ex.Message}";
        }
    }

    private async Task RedeemLoanAsync()
    {
        if (SelectedLoan is null)
        {
            StatusMessage = "Select a loan to redeem.";
            return;
        }

        try
        {
            await _loanService.RedeemLoanAsync(SelectedLoan.Id, RedeemerName, RedeemerAddress);
            RedeemerName = string.Empty;
            RedeemerAddress = string.Empty;
            StatusMessage = "Loan redeemed successfully.";
            await LoadLoansAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error redeeming loan: {ex.Message}";
        }
    }

    private async Task AuctionLoanAsync()
    {
        if (SelectedLoan is null)
        {
            StatusMessage = "Select a loan to auction.";
            return;
        }

        try
        {
            await _loanService.AuctionLoanAsync(SelectedLoan.Id);
            StatusMessage = "Loan marked as auctioned.";
            await LoadLoansAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error auctioning loan: {ex.Message}";
        }
    }

    private async Task SearchAsync()
    {
        try
        {
            var results = await _loanService.SearchLoansAsync(SearchText);
            Loans = new ObservableCollection<Loan>(results);
            StatusMessage = $"Found {results.Count} loan(s).";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error searching: {ex.Message}";
        }
    }

    private void ClearForm()
    {
        SelectedCustomer = null;
        LoanDate = DateTime.Today;
        PrincipalAmount = 0;
        InterestRate = 0;
        Weight = 0;
        PresentValue = 0;
        ArticleDescription = string.Empty;
        OwnerName = string.Empty;
        OwnerAddress = string.Empty;
        RedeemerName = string.Empty;
        RedeemerAddress = string.Empty;
    }
}
