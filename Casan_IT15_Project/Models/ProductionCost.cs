using System.ComponentModel.DataAnnotations;

namespace Casan_IT15_Project.Models
{
    /// <summary>
    /// Production cost tracking per work order.
    /// Tracks labor, material, and overhead costs.
    /// </summary>
    public class ProductionCost
    {
        [Key]
        public int CostId { get; set; }

        public int WorkOrderId { get; set; }
        public WorkOrder WorkOrder { get; set; } = null!;

        /// <summary>
        /// CostType: Labor, Material, Overhead, Equipment, Other
        /// </summary>
        [Required, MaxLength(50)]
        public string CostType { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        public decimal Amount { get; set; }

        [MaxLength(3)]
        public string Currency { get; set; } = "PHP";

        public DateTime IncurredDate { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Multi-tenant
        public int CompanyId { get; set; }
    }
}
