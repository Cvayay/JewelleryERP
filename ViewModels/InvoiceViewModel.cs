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
    private ObservableCollection<InvoiceItem> invoiceItems = new();

    [ObservableProperty]
    private ObservableCollection<Customer> customers = new();

    [ObservableProperty]
    private ObservableCollection<Product> products = new();

    [ObservableProperty]
    private ObservableCollection<Invoice> invoices = new();

    [ObservableProperty]
    private Customer? selectedCustomer;

    [ObservableProperty]
    private Product? selectedProduct;

    [ObservableProperty]
    private InvoiceItem? selectedInvoiceItem;

    [ObservableProperty]
    private Invoice? selectedInvoice;

    [ObservableProperty]
    private int quantity = 1;

    [ObservableProperty]
    private string searchText = string.Empty;

    public ObservableCollection<InvoiceItem> InvoiceItems
    {
        get => invoiceItems;
        set
        {
            if (SetProperty(ref invoiceItems, value))
            {
                HookInvoiceItems();
                OnPropertyChanged(nameof(TotalAmount));
            }
        }
    }

    public decimal TotalAmount => InvoiceItems.Sum(item => item.LineTotal);

    public IAsyncRelayCommand AddItemCommand { get; }

    public IAsyncRelayCommand RemoveItemCommand { get; }

    public IAsyncRelayCommand SaveInvoiceCommand { get; }

    public IAsyncRelayCommand LoadInvoicesCommand { get; }

    public IAsyncRelayCommand SearchInvoiceCommand { get; }

    public IAsyncRelayCommand ExportInvoiceCommand { get; }

    public InvoiceViewModel(InvoiceService invoiceService, InvoiceExportService invoiceExportService)
    {
        _invoiceService = invoiceService;
        _invoiceExportService = invoiceExportService;

        AddItemCommand = new AsyncRelayCommand(AddItemAsync);
        RemoveItemCommand = new AsyncRelayCommand(RemoveItemAsync);
        SaveInvoiceCommand = new AsyncRelayCommand(SaveInvoiceAsync);
        LoadInvoicesCommand = new AsyncRelayCommand(LoadInvoicesAsync);
        SearchInvoiceCommand = new AsyncRelayCommand(SearchInvoiceAsync);
        ExportInvoiceCommand = new AsyncRelayCommand(ExportInvoiceAsync);

        HookInvoiceItems();
    }

    private void HookInvoiceItems()
    {
        invoiceItems.CollectionChanged -= InvoiceItems_CollectionChanged;
        invoiceItems.CollectionChanged += InvoiceItems_CollectionChanged;
    }

    private void InvoiceItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(TotalAmount));
    }

    private async Task LoadInvoicesAsync()
    {
        if (Customers.Count == 0)
        {
            Customers = new ObservableCollection<Customer>(await _invoiceService.GetCustomersAsync());
        }

        if (Products.Count == 0)
        {
            Products = new ObservableCollection<Product>(await _invoiceService.GetProductsAsync());
        }

        Invoices = new ObservableCollection<Invoice>(await _invoiceService.GetAllInvoicesAsync());
    }

    private async Task SearchInvoiceAsync()
    {
        Invoices = new ObservableCollection<Invoice>(
            await _invoiceService.SearchInvoicesAsync(SearchText));
    }

    private async Task AddItemAsync()
    {
        if (SelectedProduct is null || Quantity <= 0)
        {
            return;
        }

        if (SelectedProduct.StockQuantity < Quantity)
        {
            return;
        }

        InvoiceItems.Add(new InvoiceItem
        {
            ProductId = SelectedProduct.Id,
            ProductName = SelectedProduct.Name,
            Quantity = Quantity,
            UnitPrice = SelectedProduct.Price,
            LineTotal = Quantity * SelectedProduct.Price
        });

        Quantity = 1;
        SelectedProduct = null;
        OnPropertyChanged(nameof(TotalAmount));
        await Task.CompletedTask;
    }

    private async Task RemoveItemAsync()
    {
        if (SelectedInvoiceItem is null)
        {
            return;
        }

        InvoiceItems.Remove(SelectedInvoiceItem);
        SelectedInvoiceItem = null;
        OnPropertyChanged(nameof(TotalAmount));
        await Task.CompletedTask;
    }

    private async Task SaveInvoiceAsync()
    {
        if (SelectedCustomer is null || InvoiceItems.Count == 0)
        {
            return;
        }

        var invoice = new Invoice
        {
            CustomerId = SelectedCustomer.Id,
            InvoiceDate = DateTime.Now,
            Items = InvoiceItems
                .Select(item => new InvoiceItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                })
                .ToList()
        };

        await _invoiceService.CreateInvoiceAsync(invoice);

        InvoiceItems.Clear();
        SelectedCustomer = null;
        SelectedProduct = null;
        SelectedInvoiceItem = null;
        SelectedInvoice = null;
        Quantity = 1;

        Products = new ObservableCollection<Product>(await _invoiceService.GetProductsAsync());
        await LoadInvoicesAsync();
        OnPropertyChanged(nameof(TotalAmount));
    }

    private async Task ExportInvoiceAsync()
    {
        if (SelectedInvoice is null)
        {
            return;
        }

        await _invoiceExportService.ExportInvoiceToXpsAsync(SelectedInvoice.Id);
    }
}
