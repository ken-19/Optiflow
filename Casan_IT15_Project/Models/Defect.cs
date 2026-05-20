using System.ComponentModel.DataAnnotations;

namespace Casan_IT15_Project.Models
{
    /// <summary>
    /// Defect records linked to quality inspections.
    /// </summary>
    public class Defect
    {
        [Key]
        public int DefectId { get; set; }

        public int InspectionId { get; set; }
        public QualityInspection QualityInspection { get; set; } = null!;

        [Required, MaxLength(200)]
        public string DefectType { get; set; } = string.Empty;

        /// <summary>
        /// Severity: Low, Medium, High, Critical
        /// </summary>
        [Required, MaxLength(50)]
        public string Severity { get; set; } = "Medium";

        public int DefectCount { get; set; } = 1;

        [MaxLength(1000)]
        public string? Description { get; set; }

        [MaxLength(1000)]
        public string? CorrectiveAction { get; set; }

        /// <summary>
        /// Status: Open, InProgress, Resolved, Closed
        /// </summary>
        [MaxLength(50)]
        public string Status { get; set; } = "Open";

        public DateTime ReportedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ResolvedAt { get; set; }

        // Multi-tenant
        public int CompanyId { get; set; }
    }
}
