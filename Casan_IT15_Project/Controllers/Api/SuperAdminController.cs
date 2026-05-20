using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Casan_IT15_Project.Data;

namespace Casan_IT15_Project.Controllers.Api
{
    [ApiController]
    [Route("api/superadmin")]
    [Authorize(Roles = "Super Admin")]
    public class SuperAdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SuperAdminController(ApplicationDbContext context)
        {
            _context = context;
        }





        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var now = DateTime.UtcNow;
            var sevenDaysFromNow = now.AddDays(7);
            
            var totalCompanies = await _context.Companies.CountAsync(c => c.CompanyId != 1);
            var totalUsers = await _context.Users.CountAsync(u => u.CompanyId != 1);
            var totalAdmins = await _context.UserRoles.CountAsync(ur => ur.RoleId == 2 && ur.User.CompanyId != 1);
            
            var activeSubscriptions = await _context.Companies.CountAsync(c => c.CompanyId != 1 && c.SubscriptionPlan != "None" && c.SubscriptionExpiry >= now);
            var expiredSubscriptions = await _context.Companies.CountAsync(c => c.CompanyId != 1 && c.SubscriptionPlan != "None" && c.SubscriptionExpiry < now);
            
            var planDistribution = await _context.Companies
                .Where(c => c.CompanyId != 1 && c.SubscriptionPlan != "None")
                .GroupBy(c => c.SubscriptionPlan)
                .Select(g => new { Plan = g.Key, Count = g.Count() })
                .ToListAsync();

            var companies = await _context.Companies
                .Where(c => c.CompanyId != 1)
                .Select(c => new
                {
                    c.CompanyId,
                    c.CompanyName,
                    AdminName = _context.Users.Where(u => u.CompanyId == c.CompanyId && u.UserRoles.Any(ur => ur.RoleId == 2)).Select(u => u.FirstName + " " + u.LastName).FirstOrDefault() ?? "Unknown",
                    c.SubscriptionPlan,
                    StartDate = c.CreatedAt,
                    ExpiryDate = c.SubscriptionExpiry,
                    Status = c.SubscriptionExpiry >= now ? "Active" : "Expired",
                    IsNearExpiry = c.SubscriptionPlan != "None" && c.SubscriptionExpiry >= now && c.SubscriptionExpiry <= sevenDaysFromNow,
                    UserCount = _context.Users.Count(u => u.CompanyId == c.CompanyId),
                    c.IsActive
                })
                .OrderByDescending(c => c.StartDate)
                .ToListAsync();

            var alerts = companies
                .Where(c => c.Status == "Expired" || c.IsNearExpiry)
                .Select(c => new
                {
                    c.CompanyName,
                    c.ExpiryDate,
                    Type = c.Status == "Expired" ? "Danger" : "Warning",
                    Message = c.Status == "Expired" 
                        ? $"Subscription expired on {c.ExpiryDate:MMM dd, yyyy}" 
                        : $"Subscription expires in {(c.ExpiryDate - now)?.Days} days"
                })
                .ToList();

            return Ok(new
            {
                Stats = new { totalCompanies, totalUsers, totalAdmins, activeSubscriptions, expiredSubscriptions },
                PlanDistribution = planDistribution,
                Companies = companies,
                Alerts = alerts
            });
        }

        // ===== Companies Management =====
        [HttpGet("companies")]
        public async Task<IActionResult> GetCompanies()
        {
            var companies = await _context.Companies
                .Where(c => c.CompanyId != 1)
                .Select(c => new
                {
                    c.CompanyId,
                    c.CompanyName,
                    AdminName = _context.Users
                        .Where(u => u.CompanyId == c.CompanyId && u.UserRoles.Any(ur => ur.RoleId == 2))
                        .Select(u => u.FirstName + " " + u.LastName)
                        .FirstOrDefault() ?? "No Admin",
                    Email = c.ContactEmail,
                    c.SubscriptionPlan,
                    UserCount = _context.Users.Count(u => u.CompanyId == c.CompanyId),
                    c.IsActive,
                    c.CreatedAt
                })
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return Ok(companies);
        }

        [HttpGet("companies/{id}")]
        public async Task<IActionResult> GetCompany(int id)
        {
            var company = await _context.Companies
                .Where(c => c.CompanyId == id)
                .Select(c => new
                {
                    c.CompanyId,
                    c.CompanyName,
                    c.Address,
                    c.ContactEmail,
                    c.ContactPhone,
                    c.Industry,
                    c.IsActive,
                    c.SubscriptionPlan,
                    c.SubscriptionExpiry,
                    c.CreatedAt,
                    Admins = _context.Users
                        .Where(u => u.CompanyId == c.CompanyId && u.UserRoles.Any(ur => ur.RoleId == 2))
                        .Select(u => new { u.UserId, u.FirstName, u.LastName, u.Email })
                        .ToList(),
                    UserCount = _context.Users.Count(u => u.CompanyId == c.CompanyId)
                })
                .FirstOrDefaultAsync();

            if (company == null) return NotFound("Company not found");
            return Ok(company);
        }

        // ===== Subscriptions Management =====
        [HttpGet("subscriptions")]
        public async Task<IActionResult> GetSubscriptions()
        {
            var now = DateTime.UtcNow;
            var sevenDaysFromNow = now.AddDays(7);

            var subscriptions = await _context.Companies
                .Where(c => c.SubscriptionPlan != null && c.SubscriptionPlan != "None")
                .Select(c => new
                {
                    SubscriptionId = c.CompanyId,
                    c.CompanyName,
                    Plan = c.SubscriptionPlan,
                    StartDate = c.CreatedAt,
                    ExpiryDate = c.SubscriptionExpiry,
                    Status = c.SubscriptionExpiry == null ? "Unknown"
                           : c.SubscriptionExpiry < now ? "Expired"
                           : c.SubscriptionExpiry <= sevenDaysFromNow ? "Expiring Soon"
                           : "Active",
                    Amount = c.SubscriptionPlan == "Premium Plan" ? 4999
                           : c.SubscriptionPlan == "Standard Plan" ? 2999
                           : c.SubscriptionPlan == "Basic Plan" ? 999 : 0,
                    AutoRenew = c.IsActive
                })
                .OrderByDescending(c => c.StartDate)
                .ToListAsync();

            return Ok(subscriptions);
        }

        [HttpPut("subscriptions/{id}")]
        public async Task<IActionResult> UpdateSubscription(int id, [FromBody] UpdateSubscriptionRequest request)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null) return NotFound("Company not found");

            company.SubscriptionPlan = request.Plan;
            company.SubscriptionExpiry = request.ExpiryDate;
            company.IsActive = request.IsActive;
            company.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Subscription updated successfully" });
        }

        public class UpdateSubscriptionRequest
        {
            public string Plan { get; set; } = string.Empty;
            public DateTime? ExpiryDate { get; set; }
            public bool IsActive { get; set; }
        }

        // ===== Admin Accounts Management =====
        [HttpGet("admins")]
        public async Task<IActionResult> GetAdminAccounts()
        {
            var admins = await _context.Users
                .Where(u => u.UserRoles.Any(ur => ur.RoleId == 2) && u.CompanyId != 1) // RoleId 2 = Admin, exclude internal
                .Select(u => new
                {
                    u.UserId,
                    u.Username,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    CompanyName = u.Company != null ? u.Company.CompanyName : "No Company",
                    Role = "Admin",
                    u.IsActive,
                    LastLogin = u.UpdatedAt
                })
                .OrderByDescending(u => u.LastLogin)
                .ToListAsync();

            return Ok(admins);
        }

        // ===== Backups Management =====
        [HttpGet("backups")]
        public async Task<IActionResult> GetBackups()
        {
            var backups = await _context.Backups
                .OrderByDescending(b => b.CreatedAt)
                .Select(b => new
                {
                    b.BackupId,
                    FileName = b.BackupName + ".bak",
                    Type = b.CreatedByUserId > 0 ? "Manual" : "Full",
                    Size = b.FileSizeBytes.HasValue
                        ? (b.FileSizeBytes.Value > 1048576
                            ? $"{b.FileSizeBytes.Value / 1048576} MB"
                            : $"{b.FileSizeBytes.Value / 1024} KB")
                        : "N/A",
                    b.CreatedAt,
                    CreatedBy = _context.Users
                        .Where(u => u.UserId == b.CreatedByUserId)
                        .Select(u => u.FirstName + " " + u.LastName)
                        .FirstOrDefault() ?? "System (Auto)",
                    b.Status
                })
                .ToListAsync();

            return Ok(backups);
        }

        [HttpPost("backups")]
        public async Task<IActionResult> CreateBackup()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var backup = new Models.Backup
            {
                BackupName = $"optiflow_backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}",
                Status = "Completed",
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow,
                FilePath = $"/backups/optiflow_backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}.bak",
                FileSizeBytes = new Random().Next(230, 260) * 1048576L // Simulated size
            };

            _context.Backups.Add(backup);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                backup.BackupId,
                FileName = backup.BackupName + ".bak",
                Type = "Manual",
                Size = $"{backup.FileSizeBytes / 1048576} MB",
                backup.CreatedAt,
                CreatedBy = "Super Admin",
                backup.Status
            });
        }

        [HttpGet("backups/{id}/download")]
        public async Task<IActionResult> DownloadBackup(int id)
        {
            var backup = await _context.Backups.FindAsync(id);
            if (backup == null) return NotFound("Backup not found");

            // Generate a JSON data export as the backup file
            var exportData = new
            {
                BackupInfo = new
                {
                    backup.BackupId,
                    backup.BackupName,
                    backup.CreatedAt,
                    backup.CompletedAt,
                    ExportedAt = DateTime.UtcNow,
                    SystemVersion = "OptiFlow ERP-MES v1.0"
                },
                Companies = await _context.Companies.ToListAsync(),
                Users = await _context.Users.Select(u => new
                {
                    u.UserId, u.Username, u.Email, u.FirstName, u.LastName,
                    u.IsActive, u.CompanyId, u.CreatedAt,
                    Roles = u.UserRoles.Select(ur => ur.Role.RoleName).ToList()
                }).ToListAsync(),
                ProductionSchedules = await _context.ProductionSchedules.CountAsync(),
                WorkOrders = await _context.WorkOrders.CountAsync(),
                Materials = await _context.Materials.CountAsync(),
                InventoryItems = await _context.InventoryItems.CountAsync(),
                QualityInspections = await _context.QualityInspections.CountAsync(),
                Defects = await _context.Defects.CountAsync(),
                ProductionCosts = await _context.ProductionCosts.CountAsync()
            };

            var json = System.Text.Json.JsonSerializer.Serialize(exportData, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });

            var bytes = System.Text.Encoding.UTF8.GetBytes(json);
            return File(bytes, "application/json", $"{backup.BackupName}.json");
        }

        [HttpPost("backups/{id}/restore")]
        public async Task<IActionResult> RestoreBackup(int id)
        {
            var backup = await _context.Backups.FindAsync(id);
            if (backup == null) return NotFound("Backup not found");

            if (backup.Status != "Completed")
                return BadRequest(new { message = "Only completed backups can be restored." });

            // Log the restore action
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            _context.SystemLogs.Add(new Models.SystemLog
            {
                Action = "BackupRestore",
                LogLevel = "Warning",
                Message = $"Database restore initiated from backup: {backup.BackupName} (ID: {backup.BackupId})",
                Source = "SuperAdminController",
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = $"Restore point '{backup.BackupName}' has been logged. In a production environment, this would trigger a full database restore.",
                backupId = backup.BackupId,
                backupName = backup.BackupName,
                restoredAt = DateTime.UtcNow
            });
        }
    }
}
