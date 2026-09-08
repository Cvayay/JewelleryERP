using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JewelleryERP.Models;
using JewelleryERP.Services;

namespace JewelleryERP.ViewModels;

public partial class InvoiceViewModel : ObservableObject
{
    private readonly InvoiceService _invoiceService;
    private readonly InvoiceExportService _invoiceExportService;
    private readonly SettingService _settingService;

    private ObservableCollection<InvoiceItem> invoiceItems = new();

    // ── Reference lists ───────────────────────────────────────────────────
    [ObservableProperty] private ObservableCollection<Customer> customers = new();
    [ObservableProperty] private ObservableCollection<Product> products = new();
    [ObservableProperty] private ObservableCollection<Invoice> invoices = new();

    // ── Invoice header ────────────────────────────────────────────────────
    [ObservableProperty] private Customer? selectedCustomer;
    [ObservableProperty] private Invoice? selectedInvoice;

    // ── Item entry form ───────────────────────────────────────────────────
    [ObservableProperty] private Product? selectedProduct;
    [ObservableProperty] private InvoiceItem? selectedInvoiceItem;
    [ObservableProperty] private string description = string.Empty;
    [ObservableProperty] private string hsnCode = string.Empty;
    [ObservableProperty] private decimal grossWeightGms;
    [ObservableProperty] private decimal grossWeightMg;
    [ObservableProperty] private decimal netWeightGms;
    [ObservableProperty] private decimal netWeightMg;
    [ObservableProperty] private decimal ratePerGram;
    [ObservableProperty] private decimal makingCharge;
    [ObservableProperty] private int quantity = 1;

    // ── Search ────────────────────────────────────────────────────────────
    [ObservableProperty] private string searchText = string.Empty;

    // ── Status / Totals ───────────────────────────────────────────────────
    [ObservableProperty] private string statusMessage = string.Empty;
    [ObservableProperty] private decimal cgstRate = 1.5m;
    [ObservableProperty] private decimal sgstRate = 1.5m;

    public ObservableCollection<InvoiceItem> InvoiceItems
    {
        get => invoiceItems;
        set
        {
            if (SetProperty(ref invoiceItems, value))
            {
                HookItems();
                RefreshTotals();
            }
        }
    }

    // ── Computed totals (not stored — refreshed on every item change) ─────
    public decimal ItemsSubTotal => InvoiceItems.Sum(i => i.LineTotal);
    public decimal MakingChargeTotal => InvoiceItems.Sum(i => i.MakingCharge);
    public decimal TaxableAmount => ItemsSubTotal + MakingChargeTotal;
    public decimal CgstAmount => Math.Round(TaxableAmount * CgstRate / 100m, 2);
    public decimal SgstAmount => Math.Round(TaxableAmount * SgstRate / 100m, 2);
    public decimal TotalAmount => TaxableAmount + CgstAmount + SgstAmount;

    // ── Commands ──────────────────────────────────────────────────────────
    public IAsyncRelayCommand AddItemCommand { get; }
    public IAsyncRelayCommand RemoveItemCommand { get; }
    public IAsyncRelayCommand SaveInvoiceCommand { get; }
    public IAsyncRelayCommand LoadInvoicesCommand { get; }
    public IAsyncRelayCommand SearchInvoiceCommand { get; }
    public IAsyncRelayCommand ExportInvoiceCommand { get; }
    public IAsyncRelayCommand PrintInvoiceCommand { get; }
    public IRelayCommand ClearItemFormCommand { get; }

    public InvoiceViewModel(
        InvoiceService invoiceService,
        InvoiceExportService invoiceExportService,
        SettingService settingService)
    {
        _invoiceService = invoiceService;
        _invoiceExportService = invoiceExportService;
        _settingService = settingService;

        AddItemCommand = new AsyncRelayCommand(AddItemAsync);
        RemoveItemCommand = new AsyncRelayCommand(RemoveItemAsync);
        SaveInvoiceCommand = new AsyncRelayCommand(SaveInvoiceAsync);
        LoadInvoicesCommand = new AsyncRelayCommand(LoadInvoicesAsync);
        SearchInvoiceCommand = new AsyncRelayCommand(SearchInvoiceAsync);
        ExportInvoiceCommand = new AsyncRelayCommand(ExportInvoiceAsync);
        PrintInvoiceCommand = new AsyncRelayCommand(PrintInvoiceAsync);
        ClearItemFormCommand = new RelayCommand(ClearItemForm);

        HookItems();
    }

    // ── Load ──────────────────────────────────────────────────────────────

    private async Task LoadInvoicesAsync()
    {
        try
        {
            Customers = new ObservableCollection<Customer>(await _invoiceService.GetCustomersAsync());
            Products = new ObservableCollection<Product>(await _invoiceService.GetProductsAsync());
            Invoices = new ObservableCollection<Invoice>(await _invoiceService.GetAllInvoicesAsync());

            // Load GST rates from settings
            var settings = await _settingService.GetSettingsAsync();
            if (settings is not null)
            {
                CgstRate = settings.CGST;
                SgstRate = settings.SGST;
            }

            StatusMessage = $"Loaded {Invoices.Count} invoice(s).";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading: {ex.Message}";
        }
    }

    // ── Search ────────────────────────────────────────────────────────────

    private async Task SearchInvoiceAsync()
    {
        Invoices = new ObservableCollection<Invoice>(
            await _invoiceService.SearchInvoicesAsync(SearchText));
        StatusMessage = $"Found {Invoices.Count} invoice(s).";
    }

    // ── When product selected — auto-fill rate from settings ──────────────

    partial void OnSelectedProductChanged(Product? value)
    {
        if (value is null) return;
        if (string.IsNullOrWhiteSpace(Description))
            Description = value.Name;
        // Rate will be filled by service on save; show current price as hint
        if (RatePerGram == 0)
            RatePerGram = value.SellingPrice;
    }

    // ── Add item to working invoice ───────────────────────────────────────

    private async Task AddItemAsync()
    {
        if (SelectedProduct is null)
        {
            StatusMessage = "Select a product before adding an item.";
            return;
        }
        if (NetWeightGms == 0 && NetWeightMg == 0)
        {
            StatusMessage = "Enter net weight (Gms or Mg).";
            return;
        }
        if (RatePerGram <= 0)
        {
            StatusMessage = "Rate per gram must be greater than zero.";
            return;
        }

        var netTotal = NetWeightGms + NetWeightMg / 1000m;
        var lineTotal = Math.Round(netTotal * RatePerGram, 2);

        InvoiceItems.Add(new InvoiceItem
        {
            ProductId = SelectedProduct.Id,
            ProductName = SelectedProduct.Name,
            Description = string.IsNullOrWhiteSpace(Description) ? SelectedProduct.Name : Description,
            HsnCode = HsnCode,
            GrossWeightGms = GrossWeightGms,
            GrossWeightMg = GrossWeightMg,
            NetWeightGms = NetWeightGms,
            NetWeightMg = NetWeightMg,
            RatePerGram = RatePerGram,
            MakingCharge = MakingCharge,
            Quantity = Quantity,
            LineTotal = lineTotal,
            UnitPrice = RatePerGram
        });

        ClearItemForm();
        StatusMessage = "Item added.";
        await Task.CompletedTask;
    }

    private async Task RemoveItemAsync()
    {
        if (SelectedInvoiceItem is null)
        {
            StatusMessage = "Select an item to remove.";
            return;
        }
        InvoiceItems.Remove(SelectedInvoiceItem);
        SelectedInvoiceItem = null;
        StatusMessage = "Item removed.";
        await Task.CompletedTask;
    }

    // ── Save invoice ──────────────────────────────────────────────────────

    private async Task SaveInvoiceAsync()
    {
        if (SelectedCustomer is null)
        {
            StatusMessage = "Select a customer before saving.";
            return;
        }
        if (InvoiceItems.Count == 0)
        {
            StatusMessage = "Add at least one item before saving.";
            return;
        }

        try
        {
            var invoice = new Invoice
            {
                CustomerId = SelectedCustomer.Id,
                Items = InvoiceItems.Select(item => new InvoiceItem
                {
                    ProductId = item.ProductId,
                    Description = item.Description,
                    HsnCode = item.HsnCode,
                    GrossWeightGms = item.GrossWeightGms,
                    GrossWeightMg = item.GrossWeightMg,
                    NetWeightGms = item.NetWeightGms,
                    NetWeightMg = item.NetWeightMg,
                    RatePerGram = item.RatePerGram,
                    MakingCharge = item.MakingCharge,
                    Quantity = item.Quantity
                }).ToList()
            };

            var saved = await _invoiceService.CreateInvoiceAsync(invoice);

            InvoiceItems.Clear();
            SelectedCustomer = null;
            ClearItemForm();

            await LoadInvoicesAsync();
            SelectedInvoice = Invoices.FirstOrDefault(i => i.Id == saved.Id);
            StatusMessage = $"Bill #{saved.BillNumber} saved. Total: ₹ {saved.TotalAmount:N2}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Save failed: {ex.Message}";
        }
    }

    // ── Export / Print ────────────────────────────────────────────────────

    private async Task ExportInvoiceAsync()
    {
        if (SelectedInvoice is null) { StatusMessage = "Select an invoice to export."; return; }
        var result = await _invoiceExportService.ExportInvoiceToXpsAsync(SelectedInvoice.Id);
        StatusMessage = result.IsSuccess ? $"Exported: {result.Value}" : result.Error;
    }

    private async Task PrintInvoiceAsync()
    {
        if (SelectedInvoice is null) { StatusMessage = "Select an invoice to print."; return; }
        var result = await _invoiceExportService.PrintInvoiceAsync(SelectedInvoice.Id);
        StatusMessage = result.IsSuccess ? result.Value ?? string.Empty : result.Error;
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private void HookItems()
    {
        invoiceItems.CollectionChanged -= Items_Changed;
        invoiceItems.CollectionChanged += Items_Changed;
    }

    private void Items_Changed(object? sender, NotifyCollectionChangedEventArgs e)
        => RefreshTotals();

    private void RefreshTotals()
    {
        OnPropertyChanged(nameof(ItemsSubTotal));
        OnPropertyChanged(nameof(MakingChargeTotal));
        OnPropertyChanged(nameof(TaxableAmount));
        OnPropertyChanged(nameof(CgstAmount));
        OnPropertyChanged(nameof(SgstAmount));
        OnPropertyChanged(nameof(TotalAmount));
    }

    private void ClearItemForm()
    {
        SelectedProduct = null;
        Description = string.Empty;
        HsnCode = string.Empty;
        GrossWeightGms = 0;
        GrossWeightMg = 0;
        NetWeightGms = 0;
        NetWeightMg = 0;
        RatePerGram = 0;
        MakingCharge = 0;
        Quantity = 1;
    }
}