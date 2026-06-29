# JewelleryERP Interview Script

## Project Summary

JewelleryERP is a WPF desktop ERP built with .NET 8, MVVM, CommunityToolkit.Mvvm, Entity Framework Core, and SQLite.
It covers customer management, product inventory, invoicing, role-based login, navigation, and invoice export.

## Architecture

I used a simple MVVM plus service-layer structure.

- Views only handle layout and bindings.
- ViewModels hold screen state and commands.
- Services contain business logic and database access.
- AppDbContext handles EF Core mapping to SQLite.

This keeps the UI clean and makes the logic easier to maintain and test.

## Data Flow

The flow is:

`User action -> ViewModel command -> Service method -> EF Core DbContext -> SQLite`

For example, when saving an invoice:

1. The user adds items in the invoice screen.
2. The ViewModel creates the invoice object.
3. The service calculates line totals and invoice total.
4. The service deducts product stock in a transaction.
5. EF Core saves the invoice and invoice items to SQLite.

## Why WPF

I chose WPF because it is strong for desktop ERP applications.

- It supports rich data binding.
- It works well with MVVM.
- It is suitable for form-heavy business screens.
- It is a good fit for Windows-only internal business apps.

## Invoice Module

The invoice module uses a master-detail structure.

- The master invoice stores customer, date, and total.
- The detail records store line items.
- Each line item stores quantity, unit price, and line total.

The important business rule is that stock is reduced only when the invoice is saved, and that happens in a transaction so the data stays consistent.

## Security

The app uses role-based login with Admin and Staff roles.

- Admin can access all modules.
- Staff can create invoices but is restricted from admin-only maintenance areas.

## Challenges And Solutions

- Keeping the UI responsive: I used async database calls.
- Avoiding duplicate logic: I moved business rules into services.
- Maintaining state across screens: I introduced a single-shell navigation system.
- Keeping data consistent: I used transactions for invoice save and stock updates.
- Making the app portable: I moved the SQLite database into the user's Local AppData folder.

## Closing

This project shows practical ERP patterns: modular screens, service-based logic, EF Core persistence, and clean WPF MVVM structure.
