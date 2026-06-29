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

    public IAsyncRelayCommand LoadProductsCommand { get; }

    public IAsyncRelayCommand SearchProductsCommand { get; }

    public ProductViewModel(ProductService productService)
    {
        _productService = productService;

        LoadProductsCommand = new AsyncRelayCommand(LoadProductsAsync);
        SearchProductsCommand = new AsyncRelayCommand(SearchProductsAsync);
    }

    private async Task LoadProductsAsync()
    {
        _allProducts = await _productService.GetAllProductsAsync();
        Products = new ObservableCollection<Product>(_allProducts);
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
    }
}
