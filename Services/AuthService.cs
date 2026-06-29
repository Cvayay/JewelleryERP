using JewelleryERP.Helpers;

namespace JewelleryERP.Services;

public class AuthService
{
    private readonly Dictionary<string, (string Password, AppRole Role)> _users = new(StringComparer.OrdinalIgnoreCase)
    {
        ["admin"] = ("admin123", AppRole.Admin),
        ["staff"] = ("staff123", AppRole.Staff)
    };

    public Task<Result<AuthUser>> LoginAsync(string userName, string password)
    {
        if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
        {
            return Task.FromResult(Result<AuthUser>.Failure("Username and password are required."));
        }

        if (!_users.TryGetValue(userName.Trim(), out var user) || user.Password != password)
        {
            return Task.FromResult(Result<AuthUser>.Failure("Invalid username or password."));
        }

        return Task.FromResult(Result<AuthUser>.Success(new AuthUser(userName.Trim(), user.Role)));
    }
}
