using System.ComponentModel.DataAnnotations;

namespace Casan_IT15_Project.Models
{
    /// <summary>
    /// Many-to-many join table between Roles and Permissions.
    /// </summary>
    public class RolePermission
    {
        [Key]
        public int RolePermissionId { get; set; }

        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;

        public int PermissionId { get; set; }
        public Permission Permission { get; set; } = null!;
    }
}
