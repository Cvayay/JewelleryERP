# JewelleryERP Version 1 Delivery Plan

## Goal

Build a standalone offline desktop ERP for a jewellery shop with billing, customer records, stock, pledge/loan tracking, basic reports, admin/staff access, and local backup.

## Current Baseline

- Tech stack is .NET 8, WPF, MVVM, EF Core, and SQLite.
- Login, dashboard, customer, product/stock, invoice, settings, and export services exist.
- The app builds successfully after fixing invalid customer XAML and navigation wiring.
- Settings are reachable from the sidebar and default shop/tax/rate values are initialized on first run.

## Version 1 Must-Have Scope

1. Sales Billing
   - Automatic bill number from Settings.
   - Shop name, address, GSTIN, logo/signature/stamp placeholders.
   - Customer name and phone search.
   - Description of goods, HSN, gross weight, net weight, rate per gram, making charge.
   - Automatic item amount, CGST, SGST, and total calculation.
   - A4 half-page printable bill export.
   - Daily sales PDF/report.

2. Customer Ledger
   - Customer-wise debit, credit, and running balance.
   - Ledger entries generated from sales and loan payments.
   - Customer ledger report.

3. Pledge / Loan Book
   - Pledge number.
   - Pawner name, address, loan date, principal amount.
   - Weight, present article value, article description.
   - Owner details if owner is different from pawner.
   - Interest calculation from configurable monthly rate.
   - Payment history against loan with interest.
   - Pending, redeemed, and auctioned lifecycle.
   - Redemption/auction date and redeemer details.
   - Pending loan report and overdue alerts for 6/12 months.

4. Stock
   - Product/item stock records.
   - Stock deduction from sales.
   - Stock report.

5. System Features
   - Admin role can change settings, rates, bill number, stock, and reports.
   - Staff role has limited operational access.
   - Works without internet using local SQLite.
   - Manual and automatic backup/recovery of SQLite database files.

## Version 1 Added Features

- Email alerts for pending dues.
- WhatsApp alert workflow, preferably as a manual/open-link action first.
- Automated WhatsApp/SMS integration should be treated as AMC/version 2 unless the client confirms provider, cost, and compliance requirements.

## Recommended Build Order

1. Stabilize shell and settings.
2. Replace generic invoice fields with jewellery bill fields and calculations.
3. Build print-ready bill output.
4. Add pledge/loan screens and interest calculation.
5. Add ledger entries and customer ledger report.
6. Add daily sales, pending loan, and stock reports.
7. Add backup/restore.
8. Add alerts.
9. Package/publish the desktop app.

## Client Inputs Still Needed

- Photo of manual bill book.
- Photo of pledge book.
- Exact bill paper size and printer behavior for A4 half-page printing.
- GST rules and default CGST/SGST percentages.
- Interest rule: monthly simple interest, daily prorated, grace period, rounding.
- Overdue policy: 6 months, 12 months, or both.
- Logo/signature/stamp image.
- WhatsApp/email sending method and sender account details.
