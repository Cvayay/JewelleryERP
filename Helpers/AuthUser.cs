namespace JewelleryERP.Helpers;

public sealed class AuthUser
{
    public string UserName { get; }

    public AppRole Role { get; }

    public AuthUser(string userName, AppRole role)
    {
        UserName = userName;
        Role = role;
    }
}
