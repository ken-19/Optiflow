using System.ComponentModel.DataAnnotations;

namespace Casan_IT15_Project.Models
{
    /// <summary>
    /// System audit/activity logs.
    /// </summary>
    public class SystemLog
    {
        [Key]
        public int LogId { get; set; }

        /// <summary>
        /// LogLevel: Info, Warning, Error, Critical
        /// </summary>
        [Required, MaxLength(20)]
        public string LogLevel { get; set; } = "Info";

        [Required, MaxLength(500)]
        public string Message { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Source { get; set; }

        [MaxLength(100)]
        public string? Action { get; set; }

        public int? UserId { get; set; }

        [MaxLength(50)]
        public string? IpAddress { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Multi-tenant
        public int? CompanyId { get; set; }
    }
}
