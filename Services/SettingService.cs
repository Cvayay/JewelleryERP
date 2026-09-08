using JewelleryERP.Data;
using JewelleryERP.Models;
using Microsoft.EntityFrameworkCore;

namespace JewelleryERP.Services;

public class SettingService
{
    private readonly AppDbContext _context;

    public SettingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Setting?> GetSettingsAsync()
    {
        return await _context.Settings
            .OrderBy(setting => setting.Id)
            .FirstOrDefaultAsync();
    }

    public async Task SaveSettingsAsync(Setting settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var allSettings = await _context.Settings
            .OrderBy(setting => setting.Id)
            .ToListAsync();

        if (allSettings.Count == 0)
        {
            if (settings.CreatedAt == default)
            {
                settings.CreatedAt = DateTime.Now;
            }

            await _context.Settings.AddAsync(settings);
        }
        else
        {
            var currentSettings = allSettings[0];

            currentSettings.ShopName = settings.ShopName;
            currentSettings.Address = settings.Address;
            currentSettings.GSTIN = settings.GSTIN;
            currentSettings.CurrentGoldRate = settings.CurrentGoldRate;
            currentSettings.CurrentSilverRate = settings.CurrentSilverRate;
            currentSettings.CGST = settings.CGST;
            currentSettings.SGST = settings.SGST;
            currentSettings.CurrentBillNumber = settings.CurrentBillNumber;
            currentSettings.DefaultInterestRate = settings.DefaultInterestRate;
            currentSettings.ShopLogoPath = settings.ShopLogoPath;
            currentSettings.ShopStampPath = settings.ShopStampPath;

            foreach (var extraSettings in allSettings.Skip(1))
            {
                _context.Settings.Remove(extraSettings);
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task InitializeDefaultSettingsAsync()
    {
        var settings = await GetSettingsAsync();

        if (settings is not null)
        {
            var duplicates = await _context.Settings
                .OrderBy(setting => setting.Id)
                .Skip(1)
                .ToListAsync();

            if (duplicates.Count > 0)
            {
                _context.Settings.RemoveRange(duplicates);
                await _context.SaveChangesAsync();
            }

            return;
        }

        var defaultSettings = new Setting
        {
            ShopName = "My Jewellery Shop",
            CGST = 1.5m,
            SGST = 1.5m,
            CurrentBillNumber = 1,
            DefaultInterestRate = 2m,
            CreatedAt = DateTime.Now
        };

        await _context.Settings.AddAsync(defaultSettings);
        await _context.SaveChangesAsync();
    }
}
