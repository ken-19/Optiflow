using System.ComponentModel.DataAnnotations;

namespace Casan_IT15_Project.Models
{
    /// <summary>
    /// Represents a company/tenant in the multi-tenant ERP system.
    /// All data is isolated per company.
    /// </summary>
    public class Company
    {
        [Key]
        public int CompanyId { get; set; }

        [Required, MaxLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? ContactEmail { get; set; }

        [MaxLength(20)]
        public string? ContactPhone { get; set; }

        [MaxLength(100)]
        public string? Industry { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(50)]
        public string SubscriptionPlan { get; set; } = "None"; // Basic, Standard, Premium

        public DateTime? SubscriptionExpiry { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<ProductionSchedule> ProductionSchedules { get; set; } = new List<ProductionSchedule>();
        public ICollection<Material> Materials { get; set; } = new List<Material>();
        public ICollection<InventoryItem> InventoryItems { get; set; } = new List<InventoryItem>();
        public ICollection<WorkOrder> WorkOrders { get; set; } = new List<WorkOrder>();
    }
}
