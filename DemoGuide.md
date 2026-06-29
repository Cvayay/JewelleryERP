# JewelleryERP Demo Guide

## Screens To Capture

1. Login screen
2. Dashboard
3. Customer module
4. Product module
5. Invoice module
6. Invoice export output

## Suggested Demo Flow

1. Start at login and sign in as Admin.
2. Open the dashboard to show the navigation shell.
3. Open Customers and show add/search/delete.
4. Open Products and show stock fields.
5. Open Invoices and build an invoice live.
6. Save the invoice and show stock reduction.
7. Export the invoice and show the generated file.
8. Sign out and sign back in as Staff to show restricted access.

## What To Say

- The app is built with WPF, MVVM, EF Core, and SQLite.
- The shell uses a single window and view navigation.
- Services contain business rules and database access.
- Invoices are saved as a master-detail transaction.
- Stock is updated automatically when invoices are saved.
- The app stores its database in Local AppData so it works after publishing.

## Portfolio Tip

Show the app running from the published build, not only from Visual Studio.
That makes the project feel more real in an interview.
