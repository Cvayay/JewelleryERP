using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JewelleryERP.Helpers;
using JewelleryERP.Services;
using System.Collections.ObjectModel;

namespace JewelleryERP.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly DashboardService _dashboardService;

    [ObservableProperty]
    private int totalCustomers;

    [ObservableProperty]
    private int totalProducts;

    [ObservableProperty]
    private int totalInvoices;

    [ObservableProperty]
    private decimal todaysSalesAmount;

    [ObservableProperty]
    private decimal todaysCgstAmount;

    [ObservableProperty]
    private decimal todaysSgstAmount;

    [ObservableProperty]
    private decimal inventoryValue;

    [ObservableProperty]
    private int activePledgeCount;

    [ObservableProperty]
    private decimal loanExposure;

    [ObservableProperty]
    private decimal goldStockGrams;

    [ObservableProperty]
    private decimal silverStockGrams;

    [ObservableProperty]
    private ObservableCollection<DashboardInvoiceRow> recentInvoices = new();

    [ObservableProperty]
    private ObservableCollection<DashboardLoanRow> recentPledges = new();

    [ObservableProperty]
    private ObservableCollection<string> alerts = new();

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public IAsyncRelayCommand LoadDashboardCommand { get; }

    public DashboardViewModel(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
        LoadDashboardCommand = new AsyncRelayCommand(LoadDashboardAsync);
    }

    private async Task LoadDashboardAsync()
    {
        try
        {
            DashboardSummary summary = await _dashboardService.GetSummaryAsync();

            TotalCustomers = summary.TotalCustomers;
            TotalProducts = summary.TotalProducts;
            TotalInvoices = summary.TotalInvoices;
            TodaysSalesAmount = summary.TodaysSalesAmount;
            TodaysCgstAmount = summary.TodaysCgstAmount;
            TodaysSgstAmount = summary.TodaysSgstAmount;
            InventoryValue = summary.InventoryValue;
            ActivePledgeCount = summary.ActivePledgeCount;
            LoanExposure = summary.LoanExposure;
            GoldStockGrams = summary.GoldStockGrams;
            SilverStockGrams = summary.SilverStockGrams;
            RecentInvoices = new ObservableCollection<DashboardInvoiceRow>(summary.RecentInvoices);
            RecentPledges = new ObservableCollection<DashboardLoanRow>(summary.RecentPledges);
            Alerts = new ObservableCollection<string>(summary.Alerts);
            StatusMessage = "Dashboard data loaded.";
        }
        catch (Exception ex)
        {
            TotalCustomers = 0;
            TotalProducts = 0;
            TotalInvoices = 0;
            TodaysSalesAmount = 0;
            StatusMessage = "Dashboard loading failed. Check the application error log for details.";
            AppPaths.WriteError(nameof(DashboardViewModel), ex);
        }
    }
}
