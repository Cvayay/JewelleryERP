# JewelleryERP

JewelleryERP is a .NET 8 WPF desktop ERP for a jewellery business.

## Features

- Customer management
- Product inventory management
- Invoice master-detail billing
- Stock deduction on invoice save
- Login with Admin and Staff roles
- Sidebar navigation shell
- Search and filter support
- Invoice export to printable XPS format

## Tech Stack

- .NET 8
- WPF
- MVVM with CommunityToolkit.Mvvm
- Entity Framework Core
- SQLite

## Folder Structure

- `Models/` - Entity classes used by EF Core
- `Views/` - WPF screens and user controls
- `ViewModels/` - MVVM state and commands
- `Services/` - Database and business logic
- `Data/` - EF Core `DbContext`
- `Helpers/` - Shared utilities and support types
- `Resources/` - Global WPF styles and theme resources

## Screenshots

Add screenshots here:

- Login screen
- Dashboard
- Customer module
- Product module
- Invoice module

## Setup

### Restore and build

```powershell
dotnet restore
dotnet build -c Release
```

### Run

```powershell
dotnet run
```

### Publish

```powershell
dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
```

Published output will be available in:

```text
bin\Release\publish\
```

## Default Login

- `admin / admin123`
- `staff / staff123`

## Notes

- SQLite data is stored under the user's Local Application Data folder.
- The app creates its data and export folders automatically on startup.
