using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JewelleryERP.Models;
using JewelleryERP.Services;

namespace JewelleryERP.ViewModels;

public partial class ProductViewModel : ObservableObject
{
    private readonly ProductService _productService;
    private List<Product> _allProducts = new();

    [ObservableProperty]
    private ObservableCollection<Product> products = new();

    [ObservableProperty]
    private Product? selectedProduct;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string productName = string.Empty;

    [ObservableProperty]
    private string category = string.Empty;

    [ObservableProperty]
    private decimal price;

    [ObservableProperty]
    private int stockQuantity;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public IAsyncRelayCommand LoadProductsCommand { get; }

    public IAsyncRelayCommand SearchProductsCommand { get; }

    public IAsyncRelayCommand SaveProductCommand { get; }

    public IAsyncRelayCommand DeleteProductCommand { get; }

    public IRelayCommand ClearProductCommand { get; }

    public ProductViewModel(ProductService productService)
    {
        _productService = productService;

        LoadProductsCommand = new AsyncRelayCommand(LoadProductsAsync);
        SearchProductsCommand = new AsyncRelayCommand(SearchProductsAsync);
        SaveProductCommand = new AsyncRelayCommand(SaveProductAsync);
        DeleteProductCommand = new AsyncRelayCommand(DeleteProductAsync);
        ClearProductCommand = new RelayCommand(ClearForm);
    }

    partial void OnSelectedProductChanged(Product? value)
    {
        if (value is null)
        {
            return;
        }

        ProductName = value.Name;
        Category = value.Category ?? string.Empty;
        Price = value.Price;
        StockQuantity = value.StockQuantity;
        StatusMessage = $"Editing product #{value.Id}.";
    }

    private async Task LoadProductsAsync()
    {
        _allProducts = await _productService.GetAllProductsAsync();
        Products = new ObservableCollection<Product>(_allProducts);
        StatusMessage = $"Loaded {Products.Count} products.";
    }

    private async Task SearchProductsAsync()
    {
        if (_allProducts.Count == 0)
        {
            await LoadProductsAsync();
        }

        var term = SearchText.Trim();

        if (string.IsNullOrWhiteSpace(term))
        {
            Products = new ObservableCollection<Product>(_allProducts);
            return;
        }

        var filtered = _allProducts.Where(product =>
            product.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            (product.Category?.Contains(term, StringComparison.OrdinalIgnoreCase) == true));

        Products = new ObservableCollection<Product>(filtered);
        StatusMessage = $"Found {Products.Count} products.";
    }

    private async Task SaveProductAsync()
    {
        try
        {
            var product = new Product
            {
                Id = SelectedProduct?.Id ?? 0,
                Name = ProductName,
                Category = Category,
                Price = Price,
                StockQuantity = StockQuantity
            };

            await _productService.SaveProductAsync(product);
            ClearForm();
            await LoadProductsAsync();
            StatusMessage = "Product saved.";
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
    }

    private async Task DeleteProductAsync()
    {
        if (SelectedProduct is null)
        {
            StatusMessage = "Select a product to delete.";
            return;
        }

        try
        {
            await _productService.DeleteProductAsync(SelectedProduct.Id);
            ClearForm();
            await LoadProductsAsync();
            StatusMessage = "Product deleted.";
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
        }
    }

    private void ClearForm()
    {
        SelectedProduct = null;
        ProductName = string.Empty;
        Category = string.Empty;
        Price = 0;
        StockQuantity = 0;
        StatusMessage = string.Empty;
    }
}
