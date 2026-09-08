using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JewelleryERP.Helpers;
using JewelleryERP.Services;

namespace JewelleryERP.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly AuthService _authService;
    private readonly CurrentUserSession _session;

    [ObservableProperty]
    private string userName = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public IAsyncRelayCommand LoginCommand { get; }

    public LoginViewModel(AuthService authService, CurrentUserSession session)
    {
        _authService = authService;
        _session = session;
        LoginCommand = new AsyncRelayCommand(LoginAsync);
    }

    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Password))
        {
            StatusMessage = "Please enter username and password.";
            return;
        }

        try
        {
            var result = await _authService.LoginAsync(UserName, Password);
            if (result.IsSuccess && result.Value is not null)
            {
                _session.SignIn(result.Value.UserName, result.Value.Role);
                StatusMessage = $"Welcome, {result.Value.UserName}!";
            }
            else
            {
                StatusMessage = result.Error ?? "Login failed.";
                Password = string.Empty;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Login error: {ex.Message}";
        }
    }
}
