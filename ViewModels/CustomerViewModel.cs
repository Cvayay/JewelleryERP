using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using JewelleryERP.Models;
using JewelleryERP.Services;

namespace JewelleryERP.ViewModels;

public class CustomerViewModel : INotifyPropertyChanged
{
    private readonly CustomerService _customerService;

    public ObservableCollection<Customer> Customers { get; set; } = new();

    private Customer? _selectedCustomer;
    public Customer? SelectedCustomer
    {
        get => _selectedCustomer;
        set
        {
            _selectedCustomer = value;
            OnPropertyChanged();
            PopulateFormFromSelection();
        }
    }

    private int _id;
    public int Id { get => _id; set { _id = value; OnPropertyChanged(); } }

    private string _name = string.Empty;
    public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }

    private string? _phoneNumber;
    public string? PhoneNumber { get => _phoneNumber; set { _phoneNumber = value; OnPropertyChanged(); } }

    private string? _email;
    public string? Email { get => _email; set { _email = value; OnPropertyChanged(); } }

    private string? _gstNumber;
    public string? GstNumber { get => _gstNumber; set { _gstNumber = value; OnPropertyChanged(); } }

    private string? _address;
    public string? Address { get => _address; set { _address = value; OnPropertyChanged(); } }

    private string _searchQuery = string.Empty;
    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            _searchQuery = value;
            OnPropertyChanged();
            _ = SearchCustomersAsync();
        }
    }

    public ICommand SaveCommand { get; }
    public ICommand ClearCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand LoadCustomersCommand { get; }

    public CustomerViewModel(CustomerService customerService)
    {
        _customerService = customerService;

        SaveCommand = new RelayCommand(async () => await SaveCustomerAsync());
        ClearCommand = new RelayCommand(ClearForm);
        DeleteCommand = new RelayCommand(async () => await DeleteCustomerAsync());
        LoadCustomersCommand = new RelayCommand(async () => await LoadCustomersAsync());

        _ = LoadCustomersAsync();
    }

    public async Task LoadCustomersAsync()
    {
        var list = await _customerService.GetAllCustomersAsync();
        Customers.Clear();
        foreach (var c in list) Customers.Add(c);
    }

    private async Task SearchCustomersAsync()
    {
        var list = await _customerService.SearchCustomersAsync(SearchQuery);
        Customers.Clear();
        foreach (var c in list) Customers.Add(c);
    }

    private async Task SaveCustomerAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            MessageBox.Show("Customer Name is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (Id == 0)
        {
            var customer = new Customer
            {
                Name = Name,
                PhoneNumber = PhoneNumber,
                Email = Email,
                GstNumber = GstNumber,
                Address = Address
            };
            await _customerService.AddCustomerAsync(customer);
        }
        else
        {
            var customer = new Customer
            {
                Id = Id,
                Name = Name,
                PhoneNumber = PhoneNumber,
                Email = Email,
                GstNumber = GstNumber,
                Address = Address
            };
            await _customerService.UpdateCustomerAsync(customer);
        }

        ClearForm();
        await LoadCustomersAsync();
    }

    private async Task DeleteCustomerAsync()
    {
        if (SelectedCustomer == null) return;

        var result = MessageBox.Show($"Are you sure you want to delete '{SelectedCustomer.Name}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            await _customerService.DeleteCustomerAsync(SelectedCustomer.Id);
            ClearForm();
            await LoadCustomersAsync();
        }
    }

    private void PopulateFormFromSelection()
    {
        if (SelectedCustomer != null)
        {
            Id = SelectedCustomer.Id;
            Name = SelectedCustomer.Name;
            PhoneNumber = SelectedCustomer.PhoneNumber;
            Email = SelectedCustomer.Email;
            GstNumber = SelectedCustomer.GstNumber;
            Address = SelectedCustomer.Address;
        }
    }

    private void ClearForm()
    {
        SelectedCustomer = null;
        Id = 0;
        Name = string.Empty;
        PhoneNumber = string.Empty;
        Email = string.Empty;
        GstNumber = string.Empty;
        Address = string.Empty;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}