using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JewelleryERP.Models
{
    public class Product
    {
        // Identity
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string ProductCode { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Barcode { get; set; }

        // Basic Info
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty; // e.g., Ring, Necklace, Bangle

        // Jewellery Specs
        [Required]
        [StringLength(50)]
        public string MetalType { get; set; } = "Gold"; // Gold, Platinum, Silver

        [Required]
        [StringLength(20)]
        public string Purity { get; set; } = "22K"; // 22K, 18K, 24K, 916

        [Column(TypeName = "decimal(18, 3)")]
        public decimal Weight { get; set; }

        [Column(TypeName = "decimal(18, 3)")]
        public decimal GrossWeight { get; set; }

        [Column(TypeName = "decimal(18, 3)")]
        public decimal StoneWeight { get; set; }

        [Column(TypeName = "decimal(18, 3)")]
        public decimal NetWeight { get; set; }

        // Pricing Engine
        [Column(TypeName = "decimal(18, 2)")]
        public decimal MetalRate { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal MakingCharge { get; set; }

        [Required]
        [StringLength(20)]
        public string MakingChargeType { get; set; } = "PerGram"; // PerGram, FixedPercentage, FixedAmount

        [Column(TypeName = "decimal(18, 2)")]
        public decimal StoneCost { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal SellingPrice { get; set; }

        // Inventory & Audit
        public int Quantity { get; set; } = 1;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}