using System.ComponentModel.DataAnnotations;

namespace JewelleryERP.Models;

public class Setting
{
    public int Id { get; set; }

    [Required]
    public string ShopName { get; set; } = string.Empty;

    public string? Address { get; set; }

    public string? GSTIN { get; set; }

    public decimal CurrentGoldRate { get; set; }

    public decimal CurrentSilverRate { get; set; }

    public decimal CGST { get; set; }

    public decimal SGST { get; set; }

    public int CurrentBillNumber { get; set; }

    public decimal DefaultInterestRate { get; set; }

    public DateTime CreatedAt { get; set; }
}
