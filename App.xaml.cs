using System.Windows;
using JewelleryERP.Data;
using JewelleryERP.Services;
using JewelleryERP.ViewModels;
using JewelleryERP.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JewelleryERP;

public partial class App : Application
{
    public IServiceProvider ServiceProvider { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Catch unhandled exceptions to prevent silent crashes
        this.DispatcherUnhandledException += (sender, args) =>
        {
            MessageBox.Show($"Startup Exception:\n\n{args.Exception.Message}\n\n{args.Exception.StackTrace}",
                            "JewelleryERP - Fatal Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
            args.Handled = true;
        };

        var services = new ServiceCollection();

        // Database context
        services.AddDbContext<AppDbContext>();

        // Services
        services.AddSingleton<CurrentUserSession>();
        services.AddSingleton<AuthService>();
        services.AddSingleton<CustomerService>();
        services.AddSingleton<ProductService>();
        services.AddSingleton<InvoiceService>();
        services.AddSingleton<InvoiceExportService>();
        services.AddSingleton<DashboardService>();
        services.AddSingleton<SettingService>();

        // Views
        services.AddSingleton<LoginView>();
        services.AddSingleton<DashboardView>();
        services.AddSingleton<CustomerView>();
        services.AddSingleton<ProductView>();
        services.AddSingleton<InvoiceView>();

        // ViewModels
        services.AddSingleton<LoginViewModel>();
        services.AddSingleton<CustomerViewModel>();
        services.AddSingleton<ProductViewModel>();
        services.AddSingleton<InvoiceViewModel>();
        services.AddSingleton<DashboardViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<MainViewModel>();

        // MainWindow Registration
        services.AddSingleton<MainWindow>(sp =>
        {
            var window = new MainWindow();
            window.DataContext = sp.GetRequiredService<MainViewModel>();
            return window;
        });

        ServiceProvider = services.BuildServiceProvider();

        // Apply pending EF Core migrations to ensure database tables exist before resolving ViewModels
        using (var scope = ServiceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.Migrate();
        }

        // Launch Application
        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }
}