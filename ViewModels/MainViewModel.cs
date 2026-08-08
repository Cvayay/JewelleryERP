using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JewelleryERP.Helpers;
using JewelleryERP.Services;
using JewelleryERP.Views;

namespace JewelleryERP.ViewModels;

public partial class MainViewModel : ObservableObject
{
    // UNCOMMENTED THESE FIELDS:
    private readonly CurrentUserSession _session;
    private readonly LoginView _loginView;
    private readonly DashboardView _dashboardView;
    private readonly CustomerView _customerView;
    private readonly ProductView _productView;
    private readonly InvoiceView _invoiceView;

    private readonly LoginViewModel _loginViewModel;
    private readonly CustomerViewModel _customerViewModel;
    private readonly ProductViewModel _productViewModel;
    private readonly InvoiceViewModel _invoiceViewModel;

    [ObservableProperty]
    private object? currentView;

    public bool IsAuthenticated => _session.IsAuthenticated;

    public bool IsAdmin => _session.Role == AppRole.Admin; // Ensure AppRole.Admin matches your enum/string

    public string CurrentUserName => _session.UserName ?? string.Empty;

    public string CurrentRole => _session.Role?.ToString() ?? string.Empty;

    public IAsyncRelayCommand ShowDashboardViewCommand { get; }
    public IAsyncRelayCommand ShowCustomerViewCommand { get; }
    public IAsyncRelayCommand ShowProductViewCommand { get; }
    public IAsyncRelayCommand ShowInvoiceViewCommand { get; }
    public IAsyncRelayCommand LogoutCommand { get; }

    public MainViewModel(
        CurrentUserSession session,
        LoginView loginView,
        DashboardView dashboardView,
        CustomerView customerView,
        ProductView productView,
        InvoiceView invoiceView,
        LoginViewModel loginViewModel,
        CustomerViewModel customerViewModel,
        ProductViewModel productViewModel,
        InvoiceViewModel invoiceViewModel)
    {
        _session = session;
        _loginView = loginView;
        _dashboardView = dashboardView;
        _customerView = customerView;
        _productView = productView;
        _invoiceView = invoiceView;
        
        _loginViewModel = loginViewModel;
        _customerViewModel = customerViewModel;
        _productViewModel = productViewModel;
        _invoiceViewModel = invoiceViewModel;

        _loginView.DataContext = _loginViewModel;
        // _dashboardView.DataContext = this;
        // _customerView.DataContext = _customerViewModel;
        _productView.DataContext = _productViewModel;
        _invoiceView.DataContext = _invoiceViewModel;

        _session.PropertyChanged += Session_PropertyChanged;

        ShowDashboardViewCommand = new AsyncRelayCommand(ShowDashboardViewAsync);
        ShowCustomerViewCommand = new AsyncRelayCommand(ShowCustomerViewAsync);
        ShowProductViewCommand = new AsyncRelayCommand(ShowProductViewAsync);
        ShowInvoiceViewCommand = new AsyncRelayCommand(ShowInvoiceViewAsync);
        LogoutCommand = new AsyncRelayCommand(LogoutAsync);

        // Start by checking authentication
        CurrentView = IsAuthenticated ? _dashboardView : _loginView;
    }

    private void Session_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(CurrentUserSession.UserName) or nameof(CurrentUserSession.Role))
        {
            OnPropertyChanged(nameof(IsAuthenticated));
            OnPropertyChanged(nameof(IsAdmin));
            OnPropertyChanged(nameof(CurrentUserName));
            OnPropertyChanged(nameof(CurrentRole));

            // Automatically switch views when login state changes!
            CurrentView = IsAuthenticated ? _dashboardView : _loginView;
        }
    }

    private Task ShowDashboardViewAsync()
    {
        if (!IsAuthenticated)
        {
            CurrentView = _loginView;
            return Task.CompletedTask;
        }
        CurrentView = _dashboardView;
        return Task.CompletedTask;
    }

    private async Task ShowCustomerViewAsync()
    {
        if (!IsAdmin) return;
        CurrentView = _customerView;
        await _customerViewModel.LoadCustomersCommand.ExecuteAsync(null);
    }

    private async Task ShowProductViewAsync()
    {
        if (!IsAdmin) return;
        CurrentView = _productView;
        await _productViewModel.LoadProductsCommand.ExecuteAsync(null);
    }

    private async Task ShowInvoiceViewAsync()
    {
        if (!IsAuthenticated)
        {
            CurrentView = _loginView;
            return;
        }
        CurrentView = _invoiceView;
        await _invoiceViewModel.LoadInvoicesCommand.ExecuteAsync(null);
    }

    private Task LogoutAsync()
    {
        _session.SignOut(); // This will trigger PropertyChanged and navigate to Login
        return Task.CompletedTask;
    }
}