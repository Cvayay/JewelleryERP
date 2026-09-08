using System.ComponentModel.DataAnnotations;

namespace JewelleryERP.Models;

/// <summary>
/// Audit log for tracking admin actions and system changes.
/// </summary>
public class AuditLog
{
    public int Id { get; set; }

    /// <summary>
    /// Username who performed the action.
    /// </summary>
    [Required]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Action type: Login, Logout, ChangePassword, AddCustomer, UpdateCustomer, DeleteCustomer, etc.
    /// </summary>
    [Required]
    public string ActionType { get; set; } = string.Empty;

    /// <summary>
    /// Description of what was changed (entity type and ID).
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Previous value (for updates). Can be JSON for complex objects.
    /// </summary>
    public string? OldValue { get; set; }

    /// <summary>
    /// New value (for updates). Can be JSON for complex objects.
    /// </summary>
    public string? NewValue { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? IPAddress { get; set; }

    /// <summary>
    /// Success status: true if action completed successfully, false if failed.
    /// </summary>
    public bool IsSuccess { get; set; } = true;

    /// <summary>
    /// Error message if IsSuccess is false.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
