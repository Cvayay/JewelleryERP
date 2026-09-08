using System.ComponentModel.DataAnnotations;
using JewelleryERP.Helpers;

namespace JewelleryERP.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Bcrypt hashed password. Never store plaintext passwords.
    /// </summary>
    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    public AppRole Role { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastPasswordChangeAt { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public bool IsActive { get; set; } = true;
}
