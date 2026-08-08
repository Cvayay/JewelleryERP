using System.ComponentModel.DataAnnotations.Schema;

namespace JewelleryERP.Models;

public class User
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = "Admin"; // Admin, Cashier

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
}