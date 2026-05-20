using System.ComponentModel.DataAnnotations;

namespace Casan_IT15_Project.Models
{
    /// <summary>
    /// Material Requirements Planning records.
    /// Calculates what materials are needed for work orders.
    /// </summary>
    public class MrpRecord
    {
        [Key]
        public int MrpId { get; set; }

        public int MaterialId { get; set; }
        public Material Material { get; set; } = null!;

        public int? WorkOrderId { get; set; }
        public WorkOrder? WorkOrder { get; set; }

        public int RequiredQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public int ShortageQuantity => RequiredQuantity - AvailableQuantity;

        /// <summary>
        /// Status: Sufficient, Shortage, Ordered, Fulfilled
        /// </summary>
        [MaxLength(50)]
        public string Status { get; set; } = "Sufficient";

        public DateTime? RequiredDate { get; set; }
        public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;

        // Multi-tenant
        public int CompanyId { get; set; }
    }
}
