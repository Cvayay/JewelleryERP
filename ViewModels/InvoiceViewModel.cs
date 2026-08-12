using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JewelleryERP.Models;
using JewelleryERP.Services;
using System.Collections.Observable;

namespace JewelleryERP.ViewModels;

public partial class InvoiceViewModel : ObservableObject
{
    private readonly InvoiceService _invoiceService;
    private readonly CustomerService _customerService;
    private readonly ProductService _productService;

    [ObservableProperty]
    private ObservableCollection<Invoice> _invoices = new();

    [ObservableProperty]
    private ObservableCollection<Customer> _customers = new();

    [ObservableProperty]
    private ObservableCollection<Product> _products = new();

    [ObservableProperty]
    private Customer? _selectedCustomer;

    [ObservableProperty]
    private Product? _selectedProduct;

    [ObservableProperty]
    private decimal _grossWeight;

    [ObservableProperty]
    private decimal _netWeight;

    [ObservableProperty]
    private decimal _ratePerGram;

    [ObservableProperty]
    private decimal _makingCharges;

    [ObservableProperty]
    private ObservableCollection<InvoiceItem> _currentInvoiceItems = new();

    public InvoiceViewModel(
        InvoiceService invoiceService, 
        CustomerService customerService, 
        ProductService productService)
    {
        _invoiceService = invoiceService;
        _customerService = customerService;
        _productService = productService;
    }

    [RelayCommand]
    public async Task LoadInvoicesAsync()
    {
        var invoices = await _invoiceService.GetInvoicesAsync();
        Invoices = new ObservableCollection<Invoice>(invoices);

        var customers = await _customerService.GetCustomersAsync();
        Customers = new ObservableCollection<Customer>(customers);

        var products = await _productService.GetProductsAsync();
        Products = new ObservableCollection<Product>(products);
    }

    [RelayCommand]
    public void AddItemToInvoice()
    {
        if (SelectedProduct == null) return;

        var lineTotal = (NetWeight * RatePerGram) + MakingCharges;

        var item = new InvoiceItem
        {
            ProductId = SelectedProduct.Id,
            Product = SelectedProduct,
            Description = SelectedProduct.Name,
            HsnCode = SelectedProduct.HSNCode ?? string.Empty,
            GrossWeight = GrossWeight,
            NetWeight = NetWeight,
            RatePerGram = RatePerGram,
            MakingCharges = MakingCharges,
            LineTotal = lineTotal
        };

        CurrentInvoiceItems.Add(item);

        // Reset inputs
        GrossWeight = 0;
        NetWeight = 0;
        MakingCharges = 0;
    }

    [RelayCommand]
    public async Task CreateInvoiceAsync()
    {
        if (SelectedCustomer == null || !CurrentInvoiceItems.Any()) return;

        var invoice = new Invoice
        {
            CustomerId = SelectedCustomer.Id,
            InvoiceDate = DateTime.Now,
            Items = CurrentInvoiceItems.ToList()
        };

        await _invoiceService.AddInvoiceAsync(invoice);
        
        CurrentInvoiceItems.Clear();
        await LoadInvoicesAsync();
    }
}