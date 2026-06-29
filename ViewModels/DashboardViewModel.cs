using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JewelleryERP.Helpers;
using JewelleryERP.Services;

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
    private string statusMessage = string.Empty;

    public IAsyncRelayCommand LoadDashboardCommand { get; }

    public DashboardViewModel(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
        LoadDashboardCommand = new AsyncRelayCommand(LoadDashboardAsync);
    }

    private async Task LoadDashboardAsync()
    {
        DashboardSummary summary = await _dashboardService.GetSummaryAsync();

        TotalCustomers = summary.TotalCustomers;
        TotalProducts = summary.TotalProducts;
        TotalInvoices = summary.TotalInvoices;
        TodaysSalesAmount = summary.TodaysSalesAmount;
        StatusMessage = "Dashboard data loaded.";
    }
}
