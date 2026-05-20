using System.ComponentModel.DataAnnotations;

namespace Casan_IT15_Project.Models
{
    /// <summary>
    /// Work orders generated from production schedules.
    /// Tracks the lifecycle from creation to completion.
    /// </summary>
    public class WorkOrder
    {
        [Key]
        public int WorkOrderId { get; set; }

        [Required, MaxLength(50)]
        public string WorkOrderNumber { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string ProductName { get; set; } = string.Empty;

        public int Quantity { get; set; }
        public int CompletedQuantity { get; set; }

        /// <summary>
        /// Status: Pending, InProgress, Completed, OnHold, Cancelled
        /// </summary>
        [Required, MaxLength(50)]
        public string Status { get; set; } = "Pending";

        [MaxLength(50)]
        public string? Priority { get; set; } = "Medium";

        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletionDate { get; set; }

        [MaxLength(200)]
        public string? AssignedTo { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Foreign keys
        public int? ScheduleId { get; set; }
        public ProductionSchedule? ProductionSchedule { get; set; }

        public int CompanyId { get; set; }
        public Company Company { get; set; } = null!;

        // Navigation
        public ICollection<ProductionCost> ProductionCosts { get; set; } = new List<ProductionCost>();
        public ICollection<QualityInspection> QualityInspections { get; set; } = new List<QualityInspection>();
        public ICollection<MrpRecord> MrpRecords { get; set; } = new List<MrpRecord>();
    }
}
