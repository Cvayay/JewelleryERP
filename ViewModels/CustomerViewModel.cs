using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JewelleryERP.Models;
using JewelleryERP.Services;

namespace JewelleryERP.ViewModels;

public partial class CustomerViewModel : ObservableObject
{
    private readonly CustomerService _customerService;

    [ObservableProperty]
    private string customerName = string.Empty;

    [ObservableProperty]
    private string phoneNumber = string.Empty;

    [ObservableProperty]
    private string address = string.Empty;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private ObservableCollection<Customer> customers = new();

    [ObservableProperty]
    private Customer? selectedCustomer;

    public IAsyncRelayCommand LoadCustomersCommand { get; }

    public IAsyncRelayCommand AddCustomerCommand { get; }

    public IAsyncRelayCommand SearchCustomerCommand { get; }

    public IAsyncRelayCommand DeleteCustomerCommand { get; }

    public CustomerViewModel(CustomerService customerService)
    {
        _customerService = customerService;

        LoadCustomersCommand = new AsyncRelayCommand(LoadCustomersAsync);
        AddCustomerCommand = new AsyncRelayCommand(AddCustomerAsync);
        SearchCustomerCommand = new AsyncRelayCommand(SearchCustomerAsync);
        DeleteCustomerCommand = new AsyncRelayCommand(DeleteCustomerAsync);
    }

    private async Task LoadCustomersAsync()
    {
        var customers = await _customerService.GetAllCustomersAsync();
        Customers = new ObservableCollection<Customer>(customers);
    }

    private async Task AddCustomerAsync()
    {
        if (string.IsNullOrWhiteSpace(CustomerName))
        {
            return;
        }

        var customer = new Customer
        {
            Name = CustomerName.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(PhoneNumber) ? null : PhoneNumber.Trim(),
            Address = string.IsNullOrWhiteSpace(Address) ? null : Address.Trim()
        };

        await _customerService.AddCustomerAsync(customer);

        CustomerName = string.Empty;
        PhoneNumber = string.Empty;
        Address = string.Empty;

        await LoadCustomersAsync();
    }

    private async Task SearchCustomerAsync()
    {
        var customers = await _customerService.SearchCustomersAsync(SearchText);
        Customers = new ObservableCollection<Customer>(customers);
    }

    private async Task DeleteCustomerAsync()
    {
        if (SelectedCustomer is null)
        {
            return;
        }

        await _customerService.DeleteCustomerAsync(SelectedCustomer.Id);
        SelectedCustomer = null;
        await LoadCustomersAsync();
    }
}
