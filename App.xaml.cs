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

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += App_DispatcherUnhandledException;

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
        services.AddSingleton<LoanService>();

        services.AddSingleton<LoginView>();
        services.AddSingleton<DashboardView>();
        services.AddSingleton<CustomerView>();
        services.AddSingleton<ProductView>();
        services.AddSingleton<InvoiceView>();
        services.AddSingleton<SettingsView>();

        services.AddSingleton<LoginViewModel>();
        services.AddSingleton<CustomerViewModel>();
        services.AddSingleton<ProductViewModel>();
        services.AddSingleton<InvoiceViewModel>();
        services.AddSingleton<DashboardViewModel>();
        services.AddSingleton<SettingsViewModel>();
        services.AddSingleton<MainViewModel>();
        services.AddSingleton<MainWindow>();

        ServiceProvider = services.BuildServiceProvider();

        await InitializeDatabaseAsync();

        var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
        mainWindow.DataContext = ServiceProvider.GetRequiredService<MainViewModel>();
        mainWindow.Show();
    }

    private static void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        try
        {
            Helpers.AppPaths.EnsureCreated();
            var logPath = System.IO.Path.Combine(Helpers.AppPaths.AppDataRoot, "error.log");
            var message = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {e.Exception}\n";
            System.IO.File.AppendAllText(logPath, message);
            MessageBox.Show(
                "Something went wrong, but the app will stay open. Details were saved to error.log.",
                "JewelleryERP",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        catch
        {
            // Keep the app alive even if error logging itself fails.
        }

        e.Handled = true;
    }

    private async Task InitializeDatabaseAsync()
    {
        using var scope = ServiceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.EnsureCreatedAsync();
        await RepairSchemaAsync(context);

        var settingService = scope.ServiceProvider.GetRequiredService<SettingService>();
        await settingService.InitializeDefaultSettingsAsync();
    }

    private static async Task RepairSchemaAsync(AppDbContext context)
    {
        await context.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS Customers (
                Id INTEGER NOT NULL CONSTRAINT PK_Customers PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                PhoneNumber TEXT NULL,
                Address TEXT NULL,
                CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
            );
            """);

        await context.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS Products (
                Id INTEGER NOT NULL CONSTRAINT PK_Products PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Category TEXT NULL,
                Price TEXT NOT NULL,
                StockQuantity INTEGER NOT NULL DEFAULT 0,
                CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
            );
            """);

        await context.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS Invoices (
                Id INTEGER NOT NULL CONSTRAINT PK_Invoices PRIMARY KEY AUTOINCREMENT,
                InvoiceDate TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                CustomerId INTEGER NOT NULL,
                TotalAmount TEXT NOT NULL,
                CONSTRAINT FK_Invoices_Customers_CustomerId FOREIGN KEY (CustomerId) REFERENCES Customers (Id) ON DELETE RESTRICT
            );
            """);

        await context.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS InvoiceItems (
                Id INTEGER NOT NULL CONSTRAINT PK_InvoiceItems PRIMARY KEY AUTOINCREMENT,
                InvoiceId INTEGER NOT NULL,
                ProductId INTEGER NOT NULL,
                Quantity INTEGER NOT NULL,
                UnitPrice TEXT NOT NULL,
                LineTotal TEXT NOT NULL,
                CONSTRAINT FK_InvoiceItems_Invoices_InvoiceId FOREIGN KEY (InvoiceId) REFERENCES Invoices (Id) ON DELETE CASCADE,
                CONSTRAINT FK_InvoiceItems_Products_ProductId FOREIGN KEY (ProductId) REFERENCES Products (Id) ON DELETE RESTRICT
            );
            """);

        await context.Database.ExecuteSqlRawAsync(
            "CREATE INDEX IF NOT EXISTS IX_Invoices_CustomerId ON Invoices (CustomerId);");

        await context.Database.ExecuteSqlRawAsync(
            "CREATE INDEX IF NOT EXISTS IX_InvoiceItems_InvoiceId ON InvoiceItems (InvoiceId);");

        await context.Database.ExecuteSqlRawAsync(
            "CREATE INDEX IF NOT EXISTS IX_InvoiceItems_ProductId ON InvoiceItems (ProductId);");

        await context.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS Settings (
                Id INTEGER NOT NULL CONSTRAINT PK_Settings PRIMARY KEY AUTOINCREMENT,
                ShopName TEXT NOT NULL,
                Address TEXT NULL,
                GSTIN TEXT NULL,
                CurrentGoldRate TEXT NOT NULL,
                CurrentSilverRate TEXT NOT NULL,
                CGST TEXT NOT NULL,
                SGST TEXT NOT NULL,
                CurrentBillNumber INTEGER NOT NULL DEFAULT 1,
                DefaultInterestRate TEXT NOT NULL,
                CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP
            );
            """);

        await context.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS Loans (
                Id INTEGER NOT NULL CONSTRAINT PK_Loans PRIMARY KEY AUTOINCREMENT,
                LoanNumber TEXT NOT NULL,
                CustomerId INTEGER NOT NULL,
                LoanDate TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                PrincipalAmount TEXT NOT NULL,
                Weight TEXT NOT NULL,
                PresentValue TEXT NOT NULL,
                ArticleDescription TEXT NULL,
                OwnerName TEXT NULL,
                OwnerAddress TEXT NULL,
                RedeemerName TEXT NULL,
                RedeemerAddress TEXT NULL,
                RedemptionDate TEXT NULL,
                Status INTEGER NOT NULL DEFAULT 0,
                CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                CONSTRAINT FK_Loans_Customers_CustomerId FOREIGN KEY (CustomerId) REFERENCES Customers (Id) ON DELETE RESTRICT
            );
            """);

        await context.Database.ExecuteSqlRawAsync(
            "CREATE UNIQUE INDEX IF NOT EXISTS IX_Loans_LoanNumber ON Loans (LoanNumber);");

        await context.Database.ExecuteSqlRawAsync(
            "CREATE INDEX IF NOT EXISTS IX_Loans_CustomerId ON Loans (CustomerId);");

        try
        {
            await context.Database.ExecuteSqlRawAsync("ALTER TABLE Products ADD COLUMN Category TEXT NULL;");
        }
        catch
        {
            // SQLite has no ADD COLUMN IF NOT EXISTS. If it already exists, the schema is fine.
        }
    }
}
