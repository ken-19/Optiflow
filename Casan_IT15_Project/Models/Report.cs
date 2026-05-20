using System.ComponentModel.DataAnnotations;

namespace Casan_IT15_Project.Models
{
    /// <summary>
    /// Generated report metadata.
    /// </summary>
    public class Report
    {
        [Key]
        public int ReportId { get; set; }

        [Required, MaxLength(200)]
        public string ReportName { get; set; } = string.Empty;

        /// <summary>
        /// ReportType: Production, Inventory, Quality, Costing, Summary
        /// </summary>
        [Required, MaxLength(50)]
        public string ReportType { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        public int GeneratedByUserId { get; set; }

        [MaxLength(500)]
        public string? FilePath { get; set; }

        // Multi-tenant
        public int CompanyId { get; set; }
    }
}
