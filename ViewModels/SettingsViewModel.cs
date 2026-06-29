using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JewelleryERP.Models;
using JewelleryERP.Services;

namespace JewelleryERP.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly SettingService _settingService;

    [ObservableProperty]
    private string shopName = string.Empty;

    [ObservableProperty]
    private string address = string.Empty;

    [ObservableProperty]
    private string gstin = string.Empty;

    [ObservableProperty]
    private decimal currentGoldRate;

    [ObservableProperty]
    private decimal currentSilverRate;

    [ObservableProperty]
    private decimal cgst;

    [ObservableProperty]
    private decimal sgst;

    [ObservableProperty]
    private int currentBillNumber;

    [ObservableProperty]
    private decimal defaultInterestRate;

    public IAsyncRelayCommand LoadSettingsCommand { get; }

    public IAsyncRelayCommand SaveSettingsCommand { get; }

    public SettingsViewModel(SettingService settingService)
    {
        _settingService = settingService;

        LoadSettingsCommand = new AsyncRelayCommand(LoadSettingsAsync);
        SaveSettingsCommand = new AsyncRelayCommand(SaveSettingsAsync);

        _ = LoadSettingsAsync();
    }

    private async Task LoadSettingsAsync()
    {
        Setting? settings = await _settingService.GetSettingsAsync();

        if (settings is null)
        {
            return;
        }

        ShopName = settings.ShopName;
        Address = settings.Address ?? string.Empty;
        Gstin = settings.GSTIN ?? string.Empty;
        CurrentGoldRate = settings.CurrentGoldRate;
        CurrentSilverRate = settings.CurrentSilverRate;
        Cgst = settings.CGST;
        Sgst = settings.SGST;
        CurrentBillNumber = settings.CurrentBillNumber;
        DefaultInterestRate = settings.DefaultInterestRate;
    }

    private async Task SaveSettingsAsync()
    {
        var settings = new Setting
        {
            ShopName = ShopName.Trim(),
            Address = string.IsNullOrWhiteSpace(Address) ? null : Address.Trim(),
            GSTIN = string.IsNullOrWhiteSpace(Gstin) ? null : Gstin.Trim(),
            CurrentGoldRate = CurrentGoldRate,
            CurrentSilverRate = CurrentSilverRate,
            CGST = Cgst,
            SGST = Sgst,
            CurrentBillNumber = CurrentBillNumber,
            DefaultInterestRate = DefaultInterestRate
        };

        await _settingService.SaveSettingsAsync(settings);
        await LoadSettingsAsync();
    }
}
