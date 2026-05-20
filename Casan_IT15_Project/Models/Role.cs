using System.ComponentModel.DataAnnotations;

namespace Casan_IT15_Project.Models
{
    /// <summary>
    /// Defines user roles: Super Admin, Admin, Production Planner,
    /// Inventory Manager, Cost Accountant, Shop Floor Supervisor,
    /// Quality Control Inspector, Plant Manager.
    /// </summary>
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        [Required, MaxLength(100)]
        public string RoleName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
