using System.ComponentModel.DataAnnotations;

namespace Casan_IT15_Project.Models
{
    /// <summary>
    /// Quality control inspection records linked to work orders.
    /// </summary>
    public class QualityInspection
    {
        [Key]
        public int InspectionId { get; set; }

        public int WorkOrderId { get; set; }
        public WorkOrder WorkOrder { get; set; } = null!;

        [Required, MaxLength(100)]
        public string InspectorName { get; set; } = string.Empty;

        public DateTime InspectionDate { get; set; } = DateTime.UtcNow;

        public int SampleSize { get; set; }
        public int PassedCount { get; set; }
        public int FailedCount { get; set; }

        /// <summary>
        /// Result: Pass, Fail, ConditionalPass
        /// </summary>
        [Required, MaxLength(50)]
        public string Result { get; set; } = "Pass";

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Multi-tenant
        public int CompanyId { get; set; }

        // Navigation
        public ICollection<Defect> Defects { get; set; } = new List<Defect>();
    }
}
