# JewelleryERP

JewelleryERP is a Windows desktop ERP application for jewellery retailers. It combines customer records, jewellery inventory, point-of-sale invoicing, customer ledgers, pledge/loan tracking, and shop billing settings in a single WPF application.

The application is designed for local, single-store use. It uses SQLite for persistence and creates its working folders automatically under the current Windows user's Local Application Data directory.

## Highlights

- Dashboard with sales, inventory, customer, invoice, and loan summaries.
- Customer management with search and customer ledger access.
- Jewellery product catalog with barcode, metal, purity, weight, pricing, and quantity fields.
- Invoice creation using customer and product records.
- Automatic invoice totals for making charges, CGST, SGST, and the final amount.
- Stock deduction when an invoice is saved.
- XPS invoice export and direct printing through the Windows print dialog.
- Pledge/loan registration with principal amount, jewellery weight, present value, owner details, and monthly interest.
- Loan search, pending and overdue views, redemption, and auction lifecycle actions.
- Shop settings for tax rates, bill numbering, loan interest defaults, logo, and stamp images.
- Admin and Staff roles with restricted navigation for administrative modules.
- Automatic creation and compatibility repair of the local SQLite schema on startup.

## Application Modules

### Dashboard

Provides an overview of the store's current operating data through summary cards and dashboard information loaded by `DashboardService`.

### Customers

Administrators can add, edit, search, and delete customer records. Customer information includes name, phone number, address, GSTIN, and creation date.

### Products

Administrators can maintain the jewellery catalog. Product records support:

- Product code and barcode
- Name and category
- Metal type and purity
- Gross, stone, and net weights
- Metal rate, making charge, making charge type, stone cost, and selling price
- Available quantity

Products already used on an invoice cannot be deleted. Set their quantity to zero when they should no longer be available for sale.

### Invoices

Authenticated users can create and review invoices. An invoice contains a customer and one or more product line items. The billing workflow calculates item subtotal, making charge total, CGST, SGST, and grand total. Saving an invoice also updates product stock and advances the bill number configured in Settings.

Invoices can be exported as XPS files or sent to a selected Windows printer. The printed document can include the shop name, address, GSTIN, logo, stamp, customer details, bill metadata, items, and totals.

### Customer Ledger

Administrators can open customer-specific invoice history and ledger information from the customer ledger module.

### Pledge / Loan Management

Authenticated users can register and manage pledge loans. A loan can include:

- Customer and loan number
- Loan date
- Principal amount and present value
- Jewellery weight and description
- Monthly interest rate
- Owner and address details

Loan numbers are generated automatically when one is not supplied. Pending loans can be redeemed or auctioned. The application calculates accrued simple interest using the configured monthly rate and identifies loans that are overdue by six or twelve months.

### Settings

Administrators can configure shop identity and billing defaults, including shop name, address, GSTIN, current gold and silver rates, CGST, SGST, current bill number, default loan interest rate, logo path, and stamp path.

## Roles and Access

The current authentication service uses an in-memory credential list. The default development credentials are:

| Username | Password | Role |
| --- | --- | --- |
| `admin` | `shivaji17` | Admin |
| `staff` | `staff16` | Staff |

Admin-only modules:

- Customers
- Products
- Customer Ledger
- Settings

Both Admin and Staff users can access the dashboard, invoices, and pledge/loan management after signing in.

> These credentials are development defaults defined in `Services/AuthService.cs`. Change the authentication implementation before deploying the application in a production environment.

## Technology Stack

- C# and .NET 8
- WPF desktop UI
- MVVM using CommunityToolkit.Mvvm
- Entity Framework Core 8
- SQLite through `Microsoft.EntityFrameworkCore.Sqlite`
- Microsoft.Extensions.DependencyInjection for application services
- Nullable reference types and implicit usings enabled

## Requirements

- Windows 10 or later
- .NET 8 SDK
- A Windows development environment with WPF support

The project targets `net8.0-windows` and uses the WPF desktop framework, so it is not intended to run on Linux or macOS.

## Getting Started

Clone or open the repository, then run the following commands from the project directory:

### Restore dependencies

```powershell
dotnet restore
```

### Build

```powershell
dotnet build
```

For a release build:

```powershell
dotnet build -c Release
```

### Run

```powershell
dotnet run
```

The application opens as a native Windows desktop window. On first launch, it creates the local database, required tables, default settings, and export directory.

### Publish a self-contained Windows build

```powershell
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

The published files are written under:

```text
bin\Release\net8.0-windows\win-x64\publish\
```

The exact output path may vary slightly with the installed .NET SDK and publish properties.

## Data and File Locations

Runtime data is stored in:

```text
%LOCALAPPDATA%\JewelleryERP\
```

Important files and folders:

```text
%LOCALAPPDATA%\JewelleryERP\jewelleryerp.db
%LOCALAPPDATA%\JewelleryERP\Exports\
%LOCALAPPDATA%\JewelleryERP\error.log
```

- `jewelleryerp.db` is the SQLite database.
- `Exports` contains generated XPS invoice files.
- `error.log` receives unhandled WPF exception details.

The database is not stored inside the build output directory. Back up the `%LOCALAPPDATA%\JewelleryERP` directory to preserve application data and exported invoices.

## Architecture

The project follows a service-oriented MVVM structure:

```text
JewelleryERP/
|-- Data/          EF Core DbContext and database configuration
|-- Helpers/       Shared result types, roles, paths, hashing, and UI helpers
|-- Models/        Domain entities and enums
|-- Services/      Business logic and persistence operations
|-- ViewModels/    MVVM state, commands, and view orchestration
|-- Views/         WPF screens and code-behind files
|-- Resources/     Shared WPF theme resources
|-- Migrations/    EF Core migration artifacts
|-- docs/          Planning, delivery, and test documentation
|-- App.xaml       WPF application resources and startup entry point
|-- MainWindow.xaml  Main application shell
```

At startup, `App.xaml.cs` configures dependency injection, initializes the database, repairs compatible legacy SQLite schemas, initializes default settings, and opens the main window. `MainViewModel` coordinates authenticated navigation while individual services own module-specific database and business operations.

## Database Notes

The application currently initializes its runtime database with `EnsureCreatedAsync` and startup schema-repair statements. This allows older local databases to receive compatible columns without requiring a manual migration command during normal use.

The repository also contains EF Core migration files under `Migrations/` for schema history and development tooling. Do not delete the local database casually: doing so removes customers, products, invoices, loans, and settings stored on that machine.

## Troubleshooting

### The app builds but no window appears

Check whether an earlier `JewelleryERP.exe` process is still running. Stop it from Task Manager or PowerShell and run the application again:

```powershell
Get-Process JewelleryERP -ErrorAction SilentlyContinue | Stop-Process -Force
dotnet run
```

### A runtime error dialog appears

Open the log file:

```powershell
notepad "$env:LOCALAPPDATA\JewelleryERP\error.log"
```

The log contains the exception and stack trace captured by the WPF application-level error handler.

### A view cannot load existing records

Make sure the application is closed and start it again so the startup schema-repair routine can run. Preserve a backup of `%LOCALAPPDATA%\JewelleryERP` before manually changing or deleting the database.

### Invoice export cannot be found

Look in `%LOCALAPPDATA%\JewelleryERP\Exports`. The application generates sanitized XPS filenames based on the bill number, customer, and invoice date.

## Development Workflow

Useful commands:

```powershell
# Restore packages
dotnet restore

# Build with diagnostics
dotnet build

# Run without rebuilding the project
dotnet run --no-build

# Create a release build
dotnet build -c Release
```

When changing models or database mappings, verify both a new database and an existing local database. Startup schema repair is intended to be idempotent, so repeated launches should not duplicate columns or records.

## Project Status

Implemented modules include dashboard, authentication, customers, products, invoices, invoice export/printing, customer ledger, pledge/loan management, and settings. The `docs/` directory contains additional planning, delivery, test, and demonstration material.

## License

No license file is currently included in the repository. Add a license before distributing the project outside its intended development or internal-use context.
