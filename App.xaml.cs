using System.Windows;
using JewelleryERP.Data;
using JewelleryERP.Services;
using JewelleryERP.ViewModels;
using JewelleryERP.Views;
using Microsoft.Extensions.DependencyInjection;

namespace JewelleryERP;

public partial class App : Application
{
    public IServiceProvider ServiceProvider { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>();

        services.AddSingleton<CurrentUserSession>();
        services.AddSingleton<AuthService>();
        services.AddSingleton<CustomerService>();
        services.AddSingleton<ProductService>();
        services.AddSingleton<InvoiceService>();
        services.AddSingleton<InvoiceExportService>();
        services.AddSingleton<DashboardService>();
        services.AddSingleton<SettingService>();

        services.AddSingleton<LoginView>();
        services.AddSingleton<DashboardView>();
        services.AddSingleton<CustomerView>();
        services.AddSingleton<ProductView>();
        services.AddSingleton<InvoiceView>();

        services.AddSingleton<LoginViewModel>();
        services.AddSingleton<CustomerViewModel>();
        services.AddSingleton<ProductViewModel>();
        services.AddSingleton<InvoiceViewModel>();
        services.AddSingleton<DashboardViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();

        ServiceProvider = services.BuildServiceProvider();

        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.DataContext = ServiceProvider.GetRequiredService<MainViewModel>();
        mainWindow.Show();
    }
}
