using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JewelleryERP.Data;
using JewelleryERP.Models;
using Microsoft.EntityFrameworkCore;

namespace JewelleryERP.ViewModels;

public partial class InvoiceViewModel : ObservableObject
{
    // --- Data Collections ---
    public ObservableCollection<Customer> AvailableCustomers { get; } = new();
    public ObservableCollection<Product> AvailableProducts { get; } = new();
    public ObservableCollection<InvoiceItem> CartItems { get; } = new();

    // --- Bound Properties ---
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SubTotal))]
    [NotifyPropertyChangedFor(nameof(CGSTAmount))]
    [NotifyPropertyChangedFor(nameof(SGSTAmount))]
    [NotifyPropertyChangedFor(nameof(GrandTotal))]
    private Customer? _selectedCustomer;

    [ObservableProperty]
    private Product? _selectedProduct;

    [ObservableProperty]
    private int _quantityToAdd = 1;

    // --- Calculated Totals ---
    public decimal SubTotal => CartItems.Sum(item => item.LineTotal);
    
    // Hardcoded 1.5% for Om Prakash Jewellers CRD (Can also be fetched from Settings)
    public decimal CGSTAmount => SubTotal * 0.015m; 
    public decimal SGSTAmount => SubTotal * 0.015m;
    
    public decimal GrandTotal => SubTotal + CGSTAmount + SGSTAmount;

    public InvoiceViewModel()
    {
        // Constructor logic if needed
    }

    [RelayCommand]
    public async Task LoadInvoicesAsync()
    {
        using var context = new AppDbContext();
        
        // Load dropdown data for the UI
        var customers = await context.Customers.ToListAsync();
        var products = await context.Products.Where(p => p.StockQuantity > 0).ToListAsync();

        AvailableCustomers.Clear();
        foreach (var c in customers) AvailableCustomers.Add(c);

        AvailableProducts.Clear();
        foreach (var p in products) AvailableProducts.Add(p);
        
        CartItems.Clear();
    }

    [RelayCommand]
    private void AddToCart()
    {
        if (SelectedProduct == null || QuantityToAdd <= 0) return;

        // Check if stock is available
        if (QuantityToAdd > SelectedProduct.StockQuantity)
        {
            // Ideally show a warning message to the user here
            return;
        }

        var lineTotal = SelectedProduct.Price * QuantityToAdd;

        var newItem = new InvoiceItem
        {
            ProductId = SelectedProduct.Id,
            Product = SelectedProduct, // For UI Display
            HSNCode = SelectedProduct.HSNCode,
            Quantity = QuantityToAdd,
            UnitPrice = SelectedProduct.Price,
            LineTotal = lineTotal
        };

        CartItems.Add(newItem);

        // Reset inputs
        SelectedProduct = null;
        QuantityToAdd = 1;

        // Trigger UI updates for totals
        OnPropertyChanged(nameof(SubTotal));
        OnPropertyChanged(nameof(CGSTAmount));
        OnPropertyChanged(nameof(SGSTAmount));
        OnPropertyChanged(nameof(GrandTotal));
    }

    [RelayCommand]
    private async Task SaveInvoiceAsync()
    {
        if (SelectedCustomer == null || !CartItems.Any()) return;

        using var context = new AppDbContext();

        var newInvoice = new Invoice
        {
            CustomerId = SelectedCustomer.Id,
            InvoiceDate = DateTime.Now,
            TotalAmount = GrandTotal,
            Items = CartItems.ToList()
        };

        // Deduct inventory
        foreach (var item in newInvoice.Items)
        {
            var product = await context.Products.FindAsync(item.ProductId);
            if (product != null)
            {
                product.StockQuantity -= item.Quantity;
            }
        }

        context.Invoices.Add(newInvoice);
        await context.SaveChangesAsync();

        // Clear the cart for the next customer
        CartItems.Clear();
        SelectedCustomer = null;
        
        OnPropertyChanged(nameof(SubTotal));
        OnPropertyChanged(nameof(CGSTAmount));
        OnPropertyChanged(nameof(SGSTAmount));
        OnPropertyChanged(nameof(GrandTotal));

        // Note: PDF Generation logic will be called here next!
    }
}