using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JewelleryERP.Helpers;
using JewelleryERP.Services;
using JewelleryERP.Views;
using JewelleryERP.ViewModels;

namespace JewelleryERP.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly CurrentUserSession _session;
    private readonly LoginView _loginView;
    private readonly DashboardView _dashboardView;
    private readonly CustomerView _customerView;
    private readonly ProductView _productView;
    private readonly InvoiceView _invoiceView;
    private readonly LoanView _loanView;
    private readonly CustomerLedgerView _customerLedgerView;
    private readonly SettingsView _settingsView;

    private readonly LoginViewModel _loginViewModel;
    private readonly DashboardViewModel _dashboardViewModel;
    private readonly CustomerViewModel _customerViewModel;
    private readonly ProductViewModel _productViewModel;
    private readonly InvoiceViewModel _invoiceViewModel;
    private readonly LoanViewModel _loanViewModel;
    private readonly CustomerLedgerViewModel _customerLedgerViewModel;
    private readonly SettingsViewModel _settingsViewModel;

    [ObservableProperty]
    private object? currentView;

    public bool IsAuthenticated => _session.IsAuthenticated;

    public bool IsAdmin => _session.Role == AppRole.Admin;

    public string CurrentUserName => _session.UserName ?? string.Empty;

    public string CurrentRole => _session.Role?.ToString() ?? string.Empty;

    public IAsyncRelayCommand ShowDashboardViewCommand { get; }

    public IAsyncRelayCommand ShowCustomerViewCommand { get; }

    public IAsyncRelayCommand ShowProductViewCommand { get; }

    public IAsyncRelayCommand ShowInvoiceViewCommand { get; }

    public IAsyncRelayCommand ShowLoanViewCommand { get; }

    public IAsyncRelayCommand ShowCustomerLedgerViewCommand { get; }

    public IAsyncRelayCommand ShowSettingsViewCommand { get; }

    public IAsyncRelayCommand LogoutCommand { get; }

    public MainViewModel(
        CurrentUserSession session,
        LoginView loginView,
        DashboardView dashboardView,
        CustomerView customerView,
        ProductView productView,
        InvoiceView invoiceView,
        LoanView loanView,
        CustomerLedgerView customerLedgerView,
        SettingsView settingsView,
        LoginViewModel loginViewModel,
        DashboardViewModel dashboardViewModel,
        CustomerViewModel customerViewModel,
        ProductViewModel productViewModel,
        InvoiceViewModel invoiceViewModel,
        LoanViewModel loanViewModel,
        CustomerLedgerViewModel customerLedgerViewModel,
        SettingsViewModel settingsViewModel)
    {
        _session = session;
        _loginView = loginView;
        _dashboardView = dashboardView;
        _customerView = customerView;
        _productView = productView;
        _invoiceView = invoiceView;
        _loanView = loanView;
        _customerLedgerView = customerLedgerView;
        _settingsView = settingsView;
        _loginViewModel = loginViewModel;
        _dashboardViewModel = dashboardViewModel;
        _customerViewModel = customerViewModel;
        _productViewModel = productViewModel;
        _invoiceViewModel = invoiceViewModel;
        _loanViewModel = loanViewModel;
        _customerLedgerViewModel = customerLedgerViewModel;
        _settingsViewModel = settingsViewModel;

        _loginView.DataContext = _loginViewModel;
        _dashboardView.DataContext = _dashboardViewModel;
        _customerView.DataContext = _customerViewModel;
        _productView.DataContext = _productViewModel;
        _invoiceView.DataContext = _invoiceViewModel;
        _loanView.DataContext = _loanViewModel;
        _customerLedgerView.DataContext = _customerLedgerViewModel;
        _settingsView.DataContext = _settingsViewModel;

        _session.PropertyChanged += Session_PropertyChanged;

        ShowDashboardViewCommand = new AsyncRelayCommand(ShowDashboardViewAsync);
        ShowCustomerViewCommand = new AsyncRelayCommand(ShowCustomerViewAsync);
        ShowProductViewCommand = new AsyncRelayCommand(ShowProductViewAsync);
        ShowInvoiceViewCommand = new AsyncRelayCommand(ShowInvoiceViewAsync);
        ShowLoanViewCommand = new AsyncRelayCommand(ShowLoanViewAsync);
        ShowCustomerLedgerViewCommand = new AsyncRelayCommand(ShowCustomerLedgerViewAsync);
        ShowSettingsViewCommand = new AsyncRelayCommand(ShowSettingsViewAsync);
        LogoutCommand = new AsyncRelayCommand(LogoutAsync);

        CurrentView = _loginView;
    }

    private void Session_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(CurrentUserSession.UserName) or nameof(CurrentUserSession.Role))
        {
            OnPropertyChanged(nameof(IsAuthenticated));
            OnPropertyChanged(nameof(IsAdmin));
            OnPropertyChanged(nameof(CurrentUserName));
            OnPropertyChanged(nameof(CurrentRole));

            if (IsAuthenticated)
            {
                _ = ShowDashboardViewAsync();
            }
            else
            {
                CurrentView = _loginView;
            }
        }
    }

    private async Task ShowDashboardViewAsync()
    {
        if (!IsAuthenticated)
        {
            CurrentView = _loginView;
            return;
        }

        CurrentView = _dashboardView;
        await _dashboardViewModel.LoadDashboardCommand.ExecuteAsync(null);
    }

    private async Task ShowCustomerViewAsync()
    {
        if (!IsAdmin)
        {
            return;
        }

        CurrentView = _customerView;
        await _customerViewModel.LoadCustomersCommand.ExecuteAsync(null);
    }

    private async Task ShowProductViewAsync()
    {
        if (!IsAdmin)
        {
            return;
        }

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

    private async Task ShowLoanViewAsync()
    {
        if (!IsAuthenticated)
        {
            CurrentView = _loginView;
            return;
        }

        CurrentView = _loanView;
        await _loanViewModel.LoadLoansCommand.ExecuteAsync(null);
    }

    private async Task ShowCustomerLedgerViewAsync()
    {
        if (!IsAdmin)
        {
            return;
        }

        CurrentView = _customerLedgerView;
        await _customerLedgerViewModel.LoadCustomersCommand.ExecuteAsync(null);
    }

    private async Task ShowSettingsViewAsync()
    {
        if (!IsAdmin)
        {
            return;
        }

        CurrentView = _settingsView;
        await _settingsViewModel.LoadSettingsCommand.ExecuteAsync(null);
    }

    private Task LogoutAsync()
    {
        _session.SignOut();
        CurrentView = _loginView;
        return Task.CompletedTask;
    }
}
