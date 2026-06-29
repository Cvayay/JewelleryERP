using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        var result = await _authService.LoginAsync(UserName, Password);

        if (!result.IsSuccess || result.Value is null)
        {
            StatusMessage = result.Error;
            return;
        }

        _session.SignIn(result.Value.UserName, result.Value.Role);
        Password = string.Empty;
        StatusMessage = $"Welcome, {result.Value.UserName}.";
    }
}
