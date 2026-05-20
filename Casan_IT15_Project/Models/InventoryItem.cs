using System.ComponentModel.DataAnnotations;

namespace Casan_IT15_Project.Models
{
    /// <summary>
    /// Inventory stock levels for materials.
    /// </summary>
    public class InventoryItem
    {
        [Key]
        public int InventoryId { get; set; }

        public int MaterialId { get; set; }
        public Material Material { get; set; } = null!;

        public int QuantityOnHand { get; set; }
        public int QuantityReserved { get; set; }
        public int QuantityAvailable => QuantityOnHand - QuantityReserved;

        [MaxLength(100)]
        public string? WarehouseLocation { get; set; }

        [MaxLength(50)]
        public string? BatchNumber { get; set; }

        public DateTime? LastRestockedAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Multi-tenant
        public bool IsActive { get; set; } = true;
        public int CompanyId { get; set; }
        public Company Company { get; set; } = null!;
    }
}
