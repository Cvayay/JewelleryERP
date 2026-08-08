using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JewelleryERP.Data;
using JewelleryERP.Helpers;
using JewelleryERP.Models;
using JewelleryERP.Services; 

namespace JewelleryERP.ViewModels;

public partial class LoginViewModel : ObservableObject 
{
    private readonly CurrentUserSession _session;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _username = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public LoginViewModel(CurrentUserSession session)
    {
        _session = session;
        EnsureDefaultAdminExists();
    }

    private bool CanLogin()
    {
        return !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
    }

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private void Login()
    {
        ErrorMessage = string.Empty;

        using var context = new AppDbContext();
        var user = context.Users.FirstOrDefault(u => u.Username == Username && u.IsActive);

        if (user != null && PasswordHelper.VerifyPassword(user.PasswordHash, Password))
        {
            if (System.Enum.TryParse<AppRole>(user.Role!, out var appRole))
            {
                _session.SignIn(user.Username!, appRole); 
                Password = string.Empty; 
            }
            else
            {
                ErrorMessage = "User role configuration is invalid.";
            }
        }
        else
        {
            ErrorMessage = "Invalid username or password.";
        }
    }

    private void EnsureDefaultAdminExists()
    {
        using var context = new AppDbContext();
        context.Database.EnsureCreated();
        if (!context.Users.Any())
        {
            context.Users.Add(new User
            {
                Username = "admin",
                PasswordHash = PasswordHelper.HashPassword("admin123"),
                Role = "Admin",
                IsActive = true
            });
            context.SaveChanges();
        }
    }
}