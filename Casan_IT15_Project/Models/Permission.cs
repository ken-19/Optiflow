using System.ComponentModel.DataAnnotations;

namespace Casan_IT15_Project.Models
{
    /// <summary>
    /// Granular permissions that can be assigned to roles.
    /// Examples: "Users.Create", "WorkOrders.Edit", "Reports.View"
    /// </summary>
    public class Permission
    {
        [Key]
        public int PermissionId { get; set; }

        [Required, MaxLength(100)]
        public string PermissionName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Module { get; set; }

        [MaxLength(300)]
        public string? Description { get; set; }

        // Navigation
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
