using CommunityToolkit.Mvvm.ComponentModel;
using JewelleryERP.Helpers;

namespace JewelleryERP.Services;

public partial class CurrentUserSession : ObservableObject
{
    [ObservableProperty]
    private string? userName;

    [ObservableProperty]
    private AppRole? role;

    public bool IsAuthenticated => !string.IsNullOrWhiteSpace(UserName);

    partial void OnUserNameChanged(string? value)
    {
        OnPropertyChanged(nameof(IsAuthenticated));
    }

    partial void OnRoleChanged(AppRole? value)
    {
        OnPropertyChanged(nameof(IsAuthenticated));
    }

    public void SignIn(string userName, AppRole role)
    {
        UserName = userName;
        Role = role;
    }

    public void SignOut()
    {
        UserName = null;
        Role = null;
    }
}
