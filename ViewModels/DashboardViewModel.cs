using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JewelleryERP.Services;

namespace JewelleryERP.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly DashboardService _dashboardService;

    [ObservableProperty]
    private decimal _totalSales;

    [ObservableProperty]
    private int _totalCustomers;

    [ObservableProperty]
    private int _totalProducts;

    [ObservableProperty]
    private decimal _totalPendingLoans;

    public DashboardViewModel(DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [RelayCommand]
    public async Task LoadDashboardAsync()
    {
        var (totalSales, totalCustomers, totalProducts, totalPendingLoans) = 
            await _dashboardService.GetDashboardMetricsAsync();

        TotalSales = totalSales;
        TotalCustomers = totalCustomers;
        TotalProducts = totalProducts;
        TotalPendingLoans = totalPendingLoans;
    }
}