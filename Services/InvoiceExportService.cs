using JewelleryERP.Models;

namespace JewelleryERP.Services;

public class InvoiceExportService
{
    public string GenerateInvoiceTextSummary(Invoice invoice)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("=========================================");
        sb.AppendLine("         OM PRAKASH JEWELLERS            ");
        sb.AppendLine("=========================================");
        sb.AppendLine($"Bill No: {invoice.BillNumber}");
        sb.AppendLine($"Date   : {invoice.InvoiceDate:dd/MM/yyyy HH:mm}");
        sb.AppendLine($"Customer: {invoice.Customer?.Name}");
        sb.AppendLine($"Phone   : {invoice.Customer?.PhoneNumber}");
        sb.AppendLine("-----------------------------------------");
        sb.AppendLine(string.Format("{0,-15} {1,8} {2,8} {3,10}", "Item", "Net Wt", "Rate/g", "Total"));
        sb.AppendLine("-----------------------------------------");

        foreach (var item in invoice.Items)
        {
            sb.AppendLine(string.Format("{0,-15} {1,8:F3} {2,8:F2} {3,10:C2}", 
                item.Description, 
                item.NetWeight, 
                item.RatePerGram, 
                item.LineTotal));
        }

        sb.AppendLine("-----------------------------------------");
        sb.AppendLine($"Sub Total    : {invoice.SubTotal:C2}");
        sb.AppendLine($"Making Charge: {invoice.MakingChargesTotal:C2}");
        sb.AppendLine($"CGST (1.5%)  : {invoice.CgstAmount:C2}");
        sb.AppendLine($"SGST (1.5%)  : {invoice.SgstAmount:C2}");
        sb.AppendLine($"GRAND TOTAL  : {invoice.GrandTotal:C2}");
        sb.AppendLine("=========================================");

        return sb.ToString();
    }
}