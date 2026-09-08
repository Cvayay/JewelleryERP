using JewelleryERP.Helpers;
using JewelleryERP.Data;
using JewelleryERP.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Xps.Packaging;
using System.Windows.Xps;

namespace JewelleryERP.Services;

public class InvoiceExportService
{
    private readonly AppDbContext _context;
    private readonly SettingService _settingService;

    public InvoiceExportService(AppDbContext context, SettingService settingService)
    {
        _context = context;
        _settingService = settingService;
    }

    // ── Public API ────────────────────────────────────────────────────────

    public Task<Result<string>> ExportInvoiceToXpsAsync(int invoiceId)
    {
        AppPaths.EnsureCreated();
        var invoice = LoadInvoice(invoiceId).GetAwaiter().GetResult();
        if (invoice is null)
            return Task.FromResult(Result<string>.Failure($"Invoice {invoiceId} not found."));

        var fileName = $"Bill_{invoice.BillNumber}_{invoice.Customer?.Name}_{invoice.InvoiceDate:yyyyMMdd}.xps";
        var outputPath = Path.Combine(AppPaths.ExportsFolder, SanitiseFileName(fileName));
        return ExportToXpsAsync(invoiceId, outputPath);
    }

    public async Task<Result<string>> ExportToXpsAsync(int invoiceId, string outputPath)
    {
        var invoice = await LoadInvoice(invoiceId);
        if (invoice is null)
            return Result<string>.Failure($"Invoice {invoiceId} not found.");

        var settings = await _settingService.GetSettingsAsync();
        var document = BuildJewelleryBillDocument(invoice, settings);

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        using var xpsDoc = new XpsDocument(outputPath, FileAccess.ReadWrite);
        var writer = XpsDocument.CreateXpsDocumentWriter(xpsDoc);
        writer.Write(((IDocumentPaginatorSource)document).DocumentPaginator);

        return Result<string>.Success(outputPath);
    }

    /// <summary>
    /// Sends the invoice directly to the selected printer.
    /// On Windows 10+ the user can choose "Microsoft Print to PDF" to get a PDF.
    /// </summary>
    public async Task<Result<string>> PrintInvoiceAsync(int invoiceId)
    {
        var invoice = await LoadInvoice(invoiceId);
        if (invoice is null)
            return Result<string>.Failure($"Invoice {invoiceId} not found.");

        var settings = await _settingService.GetSettingsAsync();
        var document = BuildJewelleryBillDocument(invoice, settings);

        var printDialog = new PrintDialog();
        printDialog.PrintTicket.PageMediaSize = new PageMediaSize(PageMediaSizeName.ISOA4);

        if (printDialog.ShowDialog() == true)
        {
            printDialog.PrintDocument(
                ((IDocumentPaginatorSource)document).DocumentPaginator,
                $"Bill #{invoice.BillNumber}");
            return Result<string>.Success("Sent to printer.");
        }

        return Result<string>.Failure("Print cancelled.");
    }

    // ── Document builder ──────────────────────────────────────────────────

    private static FlowDocument BuildJewelleryBillDocument(Invoice invoice, Setting? settings)
    {
        var shopName = settings?.ShopName ?? "JewelleryERP";
        var shopAddress = settings?.Address ?? string.Empty;
        var gstin = settings?.GSTIN ?? string.Empty;

        var doc = new FlowDocument
        {
            PageWidth = 793,       // A4 width in WPF units (96 dpi)
            PageHeight = 1122,     // A4 height — set to 561 for A4-half (2 bills/page)
            PagePadding = new Thickness(48, 36, 48, 36),
            ColumnWidth = double.PositiveInfinity,
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 11
        };

        // ── HEADER WITH LOGO & STAMP ───────────────────────────────────────
        var headerTable = new Table { CellSpacing = 0 };
        headerTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) }); // Logo
        headerTable.Columns.Add(new TableColumn { Width = new GridLength(2, GridUnitType.Star) }); // Shop Name
        headerTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) }); // Stamp
        var headerGroup = new TableRowGroup();

        var logoCell = new TableCell { Padding = new Thickness(6), BorderThickness = new Thickness(0) };
        if (!string.IsNullOrEmpty(settings?.ShopLogoPath) && File.Exists(settings.ShopLogoPath))
        {
            try
            {
                var imgLogo = new Image { Source = new BitmapImage(new Uri(settings.ShopLogoPath)) };
                imgLogo.Width = 80;
                imgLogo.Height = 60;
                logoCell.Blocks.Add(new BlockUIContainer(imgLogo));
            }
            catch
            {
                logoCell.Blocks.Add(new Paragraph(new Run("[Logo]")) { Foreground = Brushes.LightGray });
            }
        }
        else
        {
            logoCell.Blocks.Add(new Paragraph(new Run(string.Empty)));
        }

        var shopCell = new TableCell { Padding = new Thickness(12, 6, 12, 6), BorderThickness = new Thickness(0) };
        shopCell.Blocks.Add(new Paragraph(new Run("CASH BILL / SALES BILL"))
            { FontSize = 9, Foreground = Brushes.Gray, TextAlignment = TextAlignment.Center });
        shopCell.Blocks.Add(new Paragraph(new Run(shopName))
            { FontSize = 18, FontWeight = FontWeights.Bold, Foreground = Brushes.DarkGoldenrod, TextAlignment = TextAlignment.Center });
        shopCell.Blocks.Add(new Paragraph(new Run(shopAddress))
            { FontSize = 10, TextAlignment = TextAlignment.Center });
        if (!string.IsNullOrEmpty(gstin))
        {
            shopCell.Blocks.Add(new Paragraph(new Run($"GST IN: {gstin}"))
                { FontSize = 9, Foreground = Brushes.Gray, TextAlignment = TextAlignment.Center });
        }

        var stampCell = new TableCell { Padding = new Thickness(6), BorderThickness = new Thickness(0) };
        if (!string.IsNullOrEmpty(settings?.ShopStampPath) && File.Exists(settings.ShopStampPath))
        {
            try
            {
                var imgStamp = new Image { Source = new BitmapImage(new Uri(settings.ShopStampPath)) };
                imgStamp.Width = 80;
                imgStamp.Height = 60;
                stampCell.Blocks.Add(new BlockUIContainer(imgStamp));
            }
            catch
            {
                stampCell.Blocks.Add(new Paragraph(new Run("[Stamp]")) { Foreground = Brushes.LightGray });
            }
        }
        else
        {
            stampCell.Blocks.Add(new Paragraph(new Run(string.Empty)));
        }

        var headerRow = new TableRow();
        headerRow.Cells.Add(logoCell);
        headerRow.Cells.Add(shopCell);
        headerRow.Cells.Add(stampCell);
        headerGroup.Rows.Add(headerRow);

        headerTable.RowGroups.Add(headerGroup);
        doc.Blocks.Add(headerTable);

        var headerBorder = new Paragraph { BorderBrush = Brushes.DarkGoldenrod, BorderThickness = new Thickness(0, 0, 0, 1), Margin = new Thickness(0, 0, 0, 0), Padding = new Thickness(0, 0, 0, 6) };
        headerBorder.Inlines.Add(new Run(string.Empty));
        doc.Blocks.Add(headerBorder);

        // ── BILL META (Bill No + Date + Customer) ────────────────────────────
        var metaTable = new Table { CellSpacing = 0 };
        metaTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
        metaTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
        var metaGroup = new TableRowGroup();

        var metaRow1 = new TableRow();
        metaRow1.Cells.Add(MakeCell($"M/s: {invoice.Customer?.Name}", false, TextAlignment.Left));
        metaRow1.Cells.Add(MakeCell($"Bill No.: {invoice.BillNumber}", false, TextAlignment.Right));
        metaGroup.Rows.Add(metaRow1);

        var metaRow2 = new TableRow();
        metaRow2.Cells.Add(MakeCell($"Phone: {invoice.Customer?.PhoneNumber}", false, TextAlignment.Left));
        metaRow2.Cells.Add(MakeCell($"Date: {invoice.InvoiceDate:dd-MMM-yyyy}", false, TextAlignment.Right));
        metaGroup.Rows.Add(metaRow2);

        metaTable.RowGroups.Add(metaGroup);
        doc.Blocks.Add(metaTable);

        doc.Blocks.Add(new Paragraph { Margin = new Thickness(0, 6, 0, 0) });

        // ── ITEMS TABLE ──────────────────────────────────────────────────────
        var itemTable = BuildItemTable(invoice);
        doc.Blocks.Add(itemTable);

        // ── TOTALS ───────────────────────────────────────────────────────────
        var totalsTable = new Table { CellSpacing = 0 };
        totalsTable.Columns.Add(new TableColumn { Width = new GridLength(2, GridUnitType.Star) });
        totalsTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
        var totalsGroup = new TableRowGroup();

        var itemsSubTotal = invoice.Items.Sum(i => i.LineTotal);
        AddTotalRow(totalsGroup, "Amount Total", $"₹ {itemsSubTotal:N2}");
        AddTotalRow(totalsGroup, "MC (Making Charges)", $"₹ {invoice.MakingChargeTotal:N2}");
        AddTotalRow(totalsGroup, $"CGST @ {invoice.CgstRate:N2}%", $"₹ {invoice.CgstAmount:N2}");
        AddTotalRow(totalsGroup, $"SGST @ {invoice.SgstRate:N2}%", $"₹ {invoice.SgstAmount:N2}");
        AddTotalRow(totalsGroup, "TOTAL AMOUNT", $"₹ {invoice.TotalAmount:N2}", bold: true);

        totalsTable.RowGroups.Add(totalsGroup);
        doc.Blocks.Add(totalsTable);

        // ── FOOTER ───────────────────────────────────────────────────────────
        doc.Blocks.Add(new Paragraph { Margin = new Thickness(0, 12, 0, 0) });

        var footerTable = new Table { CellSpacing = 0 };
        footerTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
        footerTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
        var footerGroup = new TableRowGroup();

        var footerRow = new TableRow();
        footerRow.Cells.Add(MakeCell("Party Signature", false, TextAlignment.Center, topBorder: true));
        footerRow.Cells.Add(MakeCell($"For {shopName}", false, TextAlignment.Center, topBorder: true));
        footerGroup.Rows.Add(footerRow);

        var sigRow = new TableRow();
        sigRow.Cells.Add(MakeCell(string.Empty, false, TextAlignment.Center));
        sigRow.Cells.Add(MakeCell("Authorised Signature", false, TextAlignment.Center));
        footerGroup.Rows.Add(sigRow);

        footerTable.RowGroups.Add(footerGroup);
        doc.Blocks.Add(footerTable);

        doc.Blocks.Add(new Paragraph(
            new Run("This is a computer-generated bill."))
        {
            TextAlignment = TextAlignment.Center,
            FontSize = 8,
            Foreground = Brushes.Gray,
            Margin = new Thickness(0, 8, 0, 0)
        });

        return doc;
    }

    private static Table BuildItemTable(Invoice invoice)
    {
        var table = new Table
        {
            CellSpacing = 0,
            BorderBrush = Brushes.DarkGoldenrod,
            BorderThickness = new Thickness(1)
        };

        // Columns: Description | HSN | Gross Wt | Net Wt | Rate/Gm | Amount
        double[] widths = { 2.5, 0.8, 1.0, 1.0, 1.0, 1.0 };
        foreach (var w in widths)
            table.Columns.Add(new TableColumn { Width = new GridLength(w, GridUnitType.Star) });

        var group = new TableRowGroup();

        // Header row
        var hdr = new TableRow { Background = Brushes.DarkGoldenrod };
        string[] headers = { "Description of Goods", "HSN\nCode", "Gross Wt.\nGms   Mg", "Net Wt.\nGms   Mg", "Rate Per\nGram (₹)", "Amount\n₹" };
        foreach (var h in headers)
        {
            var cell = new TableCell(new Paragraph(new Run(h))
            {
                TextAlignment = TextAlignment.Center,
                FontWeight = FontWeights.SemiBold,
                FontSize = 9,
                Foreground = Brushes.White,
                Margin = new Thickness(4, 3, 4, 3)
            })
            { BorderBrush = Brushes.DarkGoldenrod, BorderThickness = new Thickness(0, 0, 0.5, 0) };
            hdr.Cells.Add(cell);
        }
        group.Rows.Add(hdr);

        // Item rows (min 6 rows for print readability)
        var items = invoice.Items.ToList();
        int minRows = Math.Max(items.Count, 6);
        for (int i = 0; i < minRows; i++)
        {
            var item = i < items.Count ? items[i] : null;
            var row = new TableRow { Background = i % 2 == 0 ? Brushes.White : new SolidColorBrush(Color.FromRgb(253, 250, 240)) };

            row.Cells.Add(ItemCell(item?.Description ?? string.Empty, TextAlignment.Left));
            row.Cells.Add(ItemCell(item?.HsnCode ?? string.Empty, TextAlignment.Center));
            row.Cells.Add(ItemCell(item is not null
                ? $"{item.GrossWeightGms:N0}   {item.GrossWeightMg:N0}"
                : string.Empty, TextAlignment.Right));
            row.Cells.Add(ItemCell(item is not null
                ? $"{item.NetWeightGms:N0}   {item.NetWeightMg:N0}"
                : string.Empty, TextAlignment.Right));
            row.Cells.Add(ItemCell(item is not null ? $"₹ {item.RatePerGram:N2}" : string.Empty, TextAlignment.Right));
            row.Cells.Add(ItemCell(item is not null ? $"₹ {item.LineTotal:N2}" : string.Empty, TextAlignment.Right));

            group.Rows.Add(row);
        }

        table.RowGroups.Add(group);
        return table;
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private static TableCell MakeCell(string text, bool bold, TextAlignment align,
        bool topBorder = false)
    {
        var para = new Paragraph(new Run(text))
        {
            TextAlignment = align,
            Margin = new Thickness(4, 2, 4, 2),
            FontWeight = bold ? FontWeights.Bold : FontWeights.Normal
        };
        return new TableCell(para)
        {
            BorderBrush = Brushes.DarkGoldenrod,
            BorderThickness = topBorder ? new Thickness(0, 1, 0, 0) : new Thickness(0),
            Padding = new Thickness(4, 6, 4, 6)
        };
    }

    private static TableCell ItemCell(string text, TextAlignment align)
    {
        return new TableCell(new Paragraph(new Run(text))
        {
            TextAlignment = align,
            Margin = new Thickness(4, 1, 4, 1),
            FontSize = 10
        })
        {
            BorderBrush = new SolidColorBrush(Color.FromRgb(200, 180, 120)),
            BorderThickness = new Thickness(0, 0, 0, 0.5),
            Padding = new Thickness(4, 4, 4, 4)
        };
    }

    private static void AddTotalRow(TableRowGroup group, string label, string value, bool bold = false)
    {
        var row = new TableRow();
        row.Cells.Add(new TableCell(new Paragraph(new Run(label))
        {
            TextAlignment = TextAlignment.Right,
            FontWeight = bold ? FontWeights.Bold : FontWeights.Normal,
            Margin = new Thickness(4, 1, 8, 1)
        })
        {
            BorderBrush = new SolidColorBrush(Color.FromRgb(200, 180, 120)),
            BorderThickness = new Thickness(0, 0, 0, bold ? 1 : 0.5)
        });
        row.Cells.Add(new TableCell(new Paragraph(new Run(value))
        {
            TextAlignment = TextAlignment.Right,
            FontWeight = bold ? FontWeights.Bold : FontWeights.Normal,
            Margin = new Thickness(4, 1, 4, 1),
            FontSize = bold ? 13 : 11
        })
        {
            BorderBrush = new SolidColorBrush(Color.FromRgb(200, 180, 120)),
            BorderThickness = new Thickness(0, 0, 0, bold ? 1 : 0.5)
        });
        group.Rows.Add(row);
    }

    private async Task<Invoice?> LoadInvoice(int invoiceId)
    {
        return await _context.Invoices
            .AsNoTracking()
            .Include(i => i.Customer)
            .Include(i => i.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);
    }

    private static string SanitiseFileName(string name)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');
        return name;
    }
}