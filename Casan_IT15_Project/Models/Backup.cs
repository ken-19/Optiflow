using System.ComponentModel.DataAnnotations;

namespace Casan_IT15_Project.Models
{
    /// <summary>
    /// System backup records.
    /// </summary>
    public class Backup
    {
        [Key]
        public int BackupId { get; set; }

        [Required, MaxLength(200)]
        public string BackupName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? FilePath { get; set; }

        public long? FileSizeBytes { get; set; }

        /// <summary>
        /// Status: Pending, InProgress, Completed, Failed
        /// </summary>
        [Required, MaxLength(50)]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }

        public int CreatedByUserId { get; set; }

        // Multi-tenant
        public int? CompanyId { get; set; }
    }
}
