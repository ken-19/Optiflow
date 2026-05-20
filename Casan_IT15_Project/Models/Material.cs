using System.ComponentModel.DataAnnotations;

namespace Casan_IT15_Project.Models
{
    /// <summary>
    /// Raw materials catalog used in production.
    /// </summary>
    public class Material
    {
        [Key]
        public int MaterialId { get; set; }

        [Required, MaxLength(50)]
        public string MaterialCode { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string MaterialName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? UnitOfMeasure { get; set; }

        public decimal UnitCost { get; set; }
        public int ReorderLevel { get; set; }
        public int MinimumOrderQty { get; set; }

        [MaxLength(200)]
        public string? SupplierName { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Multi-tenant
        public int CompanyId { get; set; }
        public Company Company { get; set; } = null!;

        // Navigation
        public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
        public ICollection<MrpRecord> MrpRecords { get; set; } = new List<MrpRecord>();
    }
}
