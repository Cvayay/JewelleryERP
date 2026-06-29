using JewelleryERP.Helpers;
using JewelleryERP.Data;
using JewelleryERP.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Windows.Documents;
using System.Windows.Xps.Packaging;
using System.Windows.Xps;

namespace JewelleryERP.Services;

public class InvoiceExportService
{
    private readonly AppDbContext _context;

    public InvoiceExportService(AppDbContext context)
    {
        _context = context;
    }

    public Task<Result<string>> ExportInvoiceToXpsAsync(int invoiceId)
    {
        AppPaths.EnsureCreated();
        var outputPath = Path.Combine(AppPaths.ExportsFolder, $"Invoice_{invoiceId}.xps");
        return ExportInvoiceToXpsAsync(invoiceId, outputPath);
    }

    public async Task<Result<string>> ExportInvoiceToXpsAsync(int invoiceId, string outputPath)
    {
        var invoice = await _context.Invoices
            .AsNoTracking()
            .Include(i => i.Customer)
            .Include(i => i.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

        if (invoice is null)
        {
            return Result<string>.Failure($"Invoice with Id {invoiceId} was not found.");
        }

        var document = BuildInvoiceDocument(invoice);

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        using var xpsDocument = new XpsDocument(outputPath, FileAccess.ReadWrite);
        var writer = XpsDocument.CreateXpsDocumentWriter(xpsDocument);
        writer.Write(((IDocumentPaginatorSource)document).DocumentPaginator);

        return Result<string>.Success(outputPath);
    }

    private static FlowDocument BuildInvoiceDocument(Invoice invoice)
    {
        var document = new FlowDocument
        {
            PagePadding = new System.Windows.Thickness(40),
            FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
            FontSize = 12
        };

        document.Blocks.Add(new Paragraph(new Run("JewelleryERP Invoice"))
        {
            FontSize = 20,
            FontWeight = System.Windows.FontWeights.Bold
        });

        document.Blocks.Add(new Paragraph(new Run($"Invoice #: {invoice.Id}")));
        document.Blocks.Add(new Paragraph(new Run($"Date: {invoice.InvoiceDate:dd-MMM-yyyy}")));
        document.Blocks.Add(new Paragraph(new Run($"Customer: {invoice.Customer?.Name}")));
        document.Blocks.Add(new Paragraph(new Run($"Phone: {invoice.Customer?.PhoneNumber}")));

        var table = new Table();
        table.Columns.Add(new TableColumn());
        table.Columns.Add(new TableColumn());
        table.Columns.Add(new TableColumn());
        table.Columns.Add(new TableColumn());

        table.RowGroups.Add(new TableRowGroup());
        var header = new TableRow();
        header.Cells.Add(new TableCell(new Paragraph(new Run("Product"))));
        header.Cells.Add(new TableCell(new Paragraph(new Run("Qty"))));
        header.Cells.Add(new TableCell(new Paragraph(new Run("Unit Price"))));
        header.Cells.Add(new TableCell(new Paragraph(new Run("Line Total"))));
        table.RowGroups[0].Rows.Add(header);

        foreach (var item in invoice.Items)
        {
            var row = new TableRow();
            row.Cells.Add(new TableCell(new Paragraph(new Run(item.Product?.Name ?? string.Empty))));
            row.Cells.Add(new TableCell(new Paragraph(new Run(item.Quantity.ToString()))));
            row.Cells.Add(new TableCell(new Paragraph(new Run(item.UnitPrice.ToString("N2")))));
            row.Cells.Add(new TableCell(new Paragraph(new Run(item.LineTotal.ToString("N2")))));
            table.RowGroups[0].Rows.Add(row);
        }

        document.Blocks.Add(table);
        document.Blocks.Add(new Paragraph(new Run($"Total Amount: {invoice.TotalAmount:N2}"))
        {
            FontSize = 14,
            FontWeight = System.Windows.FontWeights.Bold
        });

        return document;
    }
}
