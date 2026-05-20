using System.ComponentModel.DataAnnotations;

namespace Casan_IT15_Project.Models
{
    /// <summary>
    /// Production schedules for planning manufacturing runs.
    /// </summary>
    public class ProductionSchedule
    {
        [Key]
        public int ScheduleId { get; set; }

        [Required, MaxLength(100)]
        public string ProductName { get; set; } = string.Empty;

        public int PlannedQuantity { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Status: Planned, InProgress, Completed, Cancelled
        /// </summary>
        [Required, MaxLength(50)]
        public string Status { get; set; } = "Planned";

        [MaxLength(50)]
        public string? Priority { get; set; } = "Medium";

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Multi-tenant
        public int CompanyId { get; set; }
        public Company Company { get; set; } = null!;

        // Created by user
        public int? CreatedByUserId { get; set; }
        public User? CreatedByUser { get; set; }

        // Navigation
        public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
    }
}
