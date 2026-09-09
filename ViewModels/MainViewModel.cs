using System.ComponentModel;
using System.Collections.ObjectModel;
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
    private readonly ThemeService _themeService;

    [ObservableProperty]
    private object? currentView;

    [ObservableProperty]
    private bool isNavigationCollapsed;

    [ObservableProperty]
    private bool isCommandPaletteOpen;

    [ObservableProperty]
    private string commandPaletteQuery = string.Empty;

    [ObservableProperty]
    private ObservableCollection<PaletteItem> commandPaletteResults = new();

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

    public IRelayCommand ToggleNavigationCommand { get; }

    public IRelayCommand OpenCommandPaletteCommand { get; }

    public IRelayCommand CloseCommandPaletteCommand { get; }

    public IRelayCommand<PaletteItem> ExecutePaletteItemCommand { get; }

    public IRelayCommand ToggleThemeCommand { get; }

    public string ThemeToggleLabel => _themeService.ToggleLabel;

    public string DatabaseHealth => "Offline SQLite | Ready";

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
        SettingsViewModel settingsViewModel,
        ThemeService themeService)
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
        _themeService = themeService;

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
        ToggleNavigationCommand = new RelayCommand(ToggleNavigation);
        OpenCommandPaletteCommand = new RelayCommand(OpenCommandPalette);
        CloseCommandPaletteCommand = new RelayCommand(CloseCommandPalette);
        ExecutePaletteItemCommand = new RelayCommand<PaletteItem>(ExecutePaletteItem);
        ToggleThemeCommand = new RelayCommand(ToggleTheme);

        CurrentView = _loginView;
        RefreshCommandPaletteResults();
    }

    partial void OnCommandPaletteQueryChanged(string value)
        => RefreshCommandPaletteResults();

    private void ToggleNavigation()
    {
        IsNavigationCollapsed = !IsNavigationCollapsed;
    }

    private void ToggleTheme()
    {
        _themeService.ToggleTheme();
        OnPropertyChanged(nameof(ThemeToggleLabel));
    }

    private void OpenCommandPalette()
    {
        IsCommandPaletteOpen = true;
        CommandPaletteQuery = string.Empty;
        RefreshCommandPaletteResults();
    }

    private void CloseCommandPalette()
    {
        IsCommandPaletteOpen = false;
    }

    private void RefreshCommandPaletteResults()
    {
        var options = new[]
        {
            new PaletteItem("Dashboard", "Open dashboard", "Dashboard"),
            new PaletteItem("Customers", "Find customers and ledgers", "Customers"),
            new PaletteItem("Products", "Find stock and catalog items", "Products"),
            new PaletteItem("Invoices", "Find bills and sales", "Invoices"),
            new PaletteItem("Pledge / Loan", "Find Form-E pledge records", "Pledge / Loan"),
            new PaletteItem("Settings", "Shop, tax, and billing settings", "Settings")
        };

        var query = CommandPaletteQuery.Trim();
        var filtered = string.IsNullOrWhiteSpace(query)
            ? options
            : options.Where(item =>
                item.Title.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                item.Description.Contains(query, StringComparison.OrdinalIgnoreCase));

        CommandPaletteResults = new ObservableCollection<PaletteItem>(filtered);
    }

    private void ExecutePaletteItem(PaletteItem? item)
    {
        if (item is null) return;

        IsCommandPaletteOpen = false;
        _ = item.Target switch
        {
            "Dashboard" => ShowDashboardViewAsync(),
            "Customers" => ShowCustomerViewAsync(),
            "Products" => ShowProductViewAsync(),
            "Invoices" => ShowInvoiceViewAsync(),
            "Pledge / Loan" => ShowLoanViewAsync(),
            "Settings" => ShowSettingsViewAsync(),
            _ => Task.CompletedTask
        };
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

public sealed record PaletteItem(string Title, string Description, string Target);
