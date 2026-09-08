# Brutal Test Report

## Tested Build

- Configuration: Release
- Runtime: win-x64 self-contained publish
- Smoke result: Published executable launched and closed with exit code 0.

## Fixed During Test Pass

1. App startup
   - Fixed unreachable Settings module.
   - Fixed Dashboard DataContext so dashboard counts load from `DashboardViewModel`.
   - Added default settings initialization.
   - Added startup schema repair for older local SQLite databases missing `Settings`, `Loans`, or `Products.Category`.

2. Customer module
   - Fixed invalid `CustomerView.xaml` that had multiple root elements.
   - Fixed `CustomerView.xaml.cs` namespace/import and constructor formatting.

3. Product / Stock module
   - Before: read-only table pretending to be stock management.
   - After: add, edit, clear, delete, search, and status feedback are available.
   - Delete is blocked when a product already appears in invoices.

4. Invoice module
   - Before: invalid actions silently did nothing.
   - After: user sees validation/status messages for missing customer, missing item, stock shortage, save result, and export result.
   - Product/customer lists refresh after saving an invoice.

5. Settings module
   - Before: save had no visible feedback and allowed unsafe values.
   - After: validates shop name, bill number, rates, GST, and interest before saving.

## Pass / Fail by Module

| Module | Status | Brutal Notes |
| --- | --- | --- |
| Login | Pass for demo | Hardcoded users are acceptable only for demo. Not acceptable for production security. |
| Dashboard | Pass after fix | Counts and today's sales can load now. |
| Customers | Pass for basic CRUD | No edit/update flow yet. Only add/search/delete. |
| Products / Stock | Pass for Version 1 basic stock | Now usable for add/edit/delete/search. Still not jewellery-weight-aware. |
| Invoice | Partial pass | Generic quantity invoice works. It is not yet the required jewellery GST bill format. |
| Invoice Export | Partial pass | XPS export exists. Client asked print/PDF bill, preferably A4 half-page. |
| Settings | Pass for baseline | Reachable and validates obvious bad values. |
| Loan / Pledge | Fail for client delivery | Domain/service exists, but there is no user-facing screen. |
| Ledger | Fail | Not implemented. |
| Reports | Fail | Daily sales, pending loan, stock, and customer ledger reports are not implemented. |
| Backup / Recovery | Fail | Not implemented. |
| WhatsApp / Email Alerts | Fail / future | Not implemented and should not be promised without provider details. |

## Remaining Client-Critical Work

1. Replace generic invoice with jewellery bill fields: bill number, HSN, gross/net weight, rate per gram, making charge, CGST, SGST, total, signature/stamp/logo.
2. Add print/PDF bill output, ideally A4 half-page.
3. Build pledge/loan UI with interest calculation and lifecycle.
4. Build customer ledger and report screens.
5. Add daily sales, pending loan, and stock reports.
6. Add backup and restore.
7. Replace hardcoded login with persisted users/password hashing before real production use.
