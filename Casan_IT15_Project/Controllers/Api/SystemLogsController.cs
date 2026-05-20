using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Casan_IT15_Project.Data;

namespace Casan_IT15_Project.Controllers.Api
{
    [ApiController]
    [Route("api/system-logs")]
    [Authorize(Roles = "Super Admin,Admin")]
    public class SystemLogsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SystemLogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult> GetLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] string? level = null)
        {
            var companyId = GetCompanyId();
            var query = _context.SystemLogs.AsQueryable();

            if (!User.IsInRole("Super Admin"))
                query = query.Where(l => l.CompanyId == companyId);

            if (!string.IsNullOrEmpty(level))
                query = query.Where(l => l.LogLevel == level);

            var total = await query.CountAsync();
            var logs = await query
                .OrderByDescending(l => l.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new
                {
                    l.LogId,
                    l.LogLevel,
                    l.Message,
                    l.Action,
                    l.CreatedAt,
                    Username = _context.Users.Where(u => u.UserId == l.UserId).Select(u => u.Username).FirstOrDefault(),
                    Role = _context.Users.Where(u => u.UserId == l.UserId)
                                         .SelectMany(u => u.UserRoles)
                                         .Select(ur => ur.Role.RoleName)
                                         .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(new { total, page, pageSize, logs });
        }

        [HttpGet("backups")]
        [Authorize(Roles = "Super Admin")]
        public async Task<ActionResult> GetBackups()
        {
            var backups = await _context.Backups
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
            return Ok(backups);
        }

        [HttpPost("backups")]
        [Authorize(Roles = "Super Admin")]
        public async Task<ActionResult> CreateBackup()
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var backup = new Models.Backup
            {
                BackupName = $"Backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}",
                Status = "Completed",
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow,
                FilePath = $"/backups/backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}.bak"
            };

            _context.Backups.Add(backup);
            await _context.SaveChangesAsync();
            return Ok(backup);
        }

        [HttpGet("seed")]
        [AllowAnonymous]
        public async Task<ActionResult> SeedJetroData()
        {
            var adminUser = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Email == "admin_jetro@gmail.com");
            if (adminUser == null) return NotFound("Admin jetro not found.");

            // --- Step 1: Ensure admin_jetro has their own company ---
            int companyId;
            var jetroCompany = await _context.Companies.FirstOrDefaultAsync(c => c.CompanyName == "Jetro Manufacturing");
            if (jetroCompany == null)
            {
                jetroCompany = new Models.Company
                {
                    CompanyName = "Jetro Manufacturing",
                    Address = "456 Production Blvd, Cebu, Philippines",
                    ContactEmail = "admin_jetro@gmail.com",
                    ContactPhone = "+63-917-123-4567",
                    Industry = "Manufacturing",
                    IsActive = true,
                    SubscriptionPlan = "Premium Plan",
                    SubscriptionExpiry = DateTime.UtcNow.AddYears(1),
                    CreatedAt = DateTime.UtcNow
                };
                _context.Companies.Add(jetroCompany);
                await _context.SaveChangesAsync();
            }
            companyId = jetroCompany.CompanyId;

            // Move admin_jetro to their own company if needed
            if (adminUser.CompanyId != companyId)
            {
                adminUser.CompanyId = companyId;
                await _context.SaveChangesAsync();
            }

            // Ensure admin_jetro has Admin role
            if (!adminUser.UserRoles.Any(ur => ur.RoleId == 2))
            {
                _context.UserRoles.Add(new Models.UserRole { UserId = adminUser.UserId, RoleId = 2, AssignedAt = DateTime.UtcNow });
                await _context.SaveChangesAsync();
            }

            // --- Step 2: Create role-based users for this company ---
            var roles = new[] {
                new { Id = 3, Name = "Production Planner", Username = "planner_jetro" },
                new { Id = 4, Name = "Inventory Manager", Username = "inventory_jetro" },
                new { Id = 5, Name = "Cost Accountant", Username = "cost_jetro" },
                new { Id = 6, Name = "Shop Floor Supervisor", Username = "supervisor_jetro" },
                new { Id = 7, Name = "Quality Control Inspector", Username = "quality_jetro" },
                new { Id = 8, Name = "Plant Manager", Username = "manager_jetro" }
            };

            foreach (var r in roles)
            {
                var existing = await _context.Users.FirstOrDefaultAsync(u => u.Username == r.Username);
                if (existing == null)
                {
                    var u = new Models.User {
                        Username = r.Username,
                        Email = r.Username + "@gmail.com",
                        FirstName = "Test",
                        LastName = r.Name,
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                        CompanyId = companyId,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Users.Add(u);
                    await _context.SaveChangesAsync();
                    _context.UserRoles.Add(new Models.UserRole { UserId = u.UserId, RoleId = r.Id, AssignedAt = DateTime.UtcNow });
                    await _context.SaveChangesAsync();
                }
                else if (existing.CompanyId != companyId)
                {
                    // Fix: move existing users to the correct company
                    existing.CompanyId = companyId;
                    await _context.SaveChangesAsync();
                }
            }

            // --- Step 3: Seed Materials (company-scoped codes) ---
            if (!await _context.Materials.AnyAsync(m => m.CompanyId == companyId))
            {
                _context.Materials.AddRange(
                    new Models.Material { MaterialCode = "JM-MAT-001", MaterialName = "Aluminum Sheet", Description = "High grade aluminum", UnitOfMeasure = "kg", UnitCost = 15.50m, CompanyId = companyId },
                    new Models.Material { MaterialCode = "JM-MAT-002", MaterialName = "Steel Bolt M8", Description = "Standard steel bolt", UnitOfMeasure = "pcs", UnitCost = 0.50m, CompanyId = companyId },
                    new Models.Material { MaterialCode = "JM-MAT-003", MaterialName = "Copper Wire", Description = "Insulated copper wire", UnitOfMeasure = "m", UnitCost = 3.20m, CompanyId = companyId }
                );
                await _context.SaveChangesAsync();
            }
            var mats = await _context.Materials.Where(m => m.CompanyId == companyId).ToListAsync();

            if (mats.Count >= 3)
            {
                // Seed Inventory Items
                if (!await _context.InventoryItems.AnyAsync(i => i.CompanyId == companyId))
                {
                    _context.InventoryItems.AddRange(
                        new Models.InventoryItem { MaterialId = mats[0].MaterialId, QuantityOnHand = 500, QuantityReserved = 0, WarehouseLocation = "Warehouse A", LastRestockedAt = DateTime.UtcNow, CompanyId = companyId },
                        new Models.InventoryItem { MaterialId = mats[1].MaterialId, QuantityOnHand = 2000, QuantityReserved = 100, WarehouseLocation = "Warehouse B", LastRestockedAt = DateTime.UtcNow, CompanyId = companyId },
                        new Models.InventoryItem { MaterialId = mats[2].MaterialId, QuantityOnHand = 50, QuantityReserved = 0, WarehouseLocation = "Warehouse A", LastRestockedAt = DateTime.UtcNow, CompanyId = companyId }
                    );
                    await _context.SaveChangesAsync();
                }

                // Seed MRP
                if (!await _context.MrpRecords.AnyAsync(m => m.CompanyId == companyId))
                {
                    _context.MrpRecords.AddRange(
                        new Models.MrpRecord { MaterialId = mats[2].MaterialId, RequiredQuantity = 300, AvailableQuantity = 50, RequiredDate = DateTime.UtcNow.AddDays(7), Status = "Shortage", CompanyId = companyId, CalculatedAt = DateTime.UtcNow },
                        new Models.MrpRecord { MaterialId = mats[0].MaterialId, RequiredQuantity = 150, AvailableQuantity = 500, RequiredDate = DateTime.UtcNow.AddDays(14), Status = "Sufficient", CompanyId = companyId, CalculatedAt = DateTime.UtcNow }
                    );
                    await _context.SaveChangesAsync();
                }
            }

            // --- Step 4: Seed Work Orders (company-scoped numbers) ---
            if (!await _context.WorkOrders.AnyAsync(w => w.CompanyId == companyId))
            {
                var woCount = await _context.WorkOrders.CountAsync();
                var wo1 = new Models.WorkOrder { WorkOrderNumber = $"JM-WO-{woCount + 1:D3}", ProductName = "Engine Block Alpha", Quantity = 100, CompletedQuantity = 45, Status = "InProgress", StartDate = DateTime.UtcNow.AddDays(-2), DueDate = DateTime.UtcNow.AddDays(5), AssignedTo = "supervisor_jetro", Notes = "Urgent batch", CompanyId = companyId };
                var wo2 = new Models.WorkOrder { WorkOrderNumber = $"JM-WO-{woCount + 2:D3}", ProductName = "Transmission Unit B", Quantity = 50, CompletedQuantity = 50, Status = "Completed", StartDate = DateTime.UtcNow.AddDays(-10), DueDate = DateTime.UtcNow.AddDays(-1), AssignedTo = "planner_jetro", Notes = "Standard run", CompanyId = companyId };
                var wo3 = new Models.WorkOrder { WorkOrderNumber = $"JM-WO-{woCount + 3:D3}", ProductName = "Chassis Frame", Quantity = 200, CompletedQuantity = 0, Status = "Pending", StartDate = DateTime.UtcNow.AddDays(2), DueDate = DateTime.UtcNow.AddDays(15), AssignedTo = "supervisor_jetro", Notes = "Awaiting materials", CompanyId = companyId };
                
                _context.WorkOrders.AddRange(wo1, wo2, wo3);
                await _context.SaveChangesAsync();

                // Production Schedules
                var ps1 = new Models.ProductionSchedule { ProductName = wo1.ProductName, PlannedQuantity = wo1.Quantity, StartDate = wo1.StartDate ?? DateTime.UtcNow, EndDate = wo1.DueDate ?? DateTime.UtcNow, Status = "InProgress", CompanyId = companyId };
                var ps2 = new Models.ProductionSchedule { ProductName = wo2.ProductName, PlannedQuantity = wo2.Quantity, StartDate = wo2.StartDate ?? DateTime.UtcNow, EndDate = wo2.DueDate ?? DateTime.UtcNow, Status = "Completed", CompanyId = companyId };
                _context.ProductionSchedules.AddRange(ps1, ps2);
                await _context.SaveChangesAsync();

                wo1.ScheduleId = ps1.ScheduleId;
                wo2.ScheduleId = ps2.ScheduleId;
                
                // Quality Inspections
                var qi1 = new Models.QualityInspection { WorkOrderId = wo1.WorkOrderId, InspectorName = "quality_jetro", InspectionDate = DateTime.UtcNow, SampleSize = 10, PassedCount = 8, FailedCount = 2, Result = "Fail", Notes = "Surface scratch", CompanyId = companyId };
                var qi2 = new Models.QualityInspection { WorkOrderId = wo2.WorkOrderId, InspectorName = "quality_jetro", InspectionDate = DateTime.UtcNow.AddDays(-2), SampleSize = 5, PassedCount = 5, FailedCount = 0, Result = "Pass", Notes = "All good", CompanyId = companyId };
                _context.QualityInspections.AddRange(qi1, qi2);
                await _context.SaveChangesAsync();

                // Defects
                _context.Defects.AddRange(
                    new Models.Defect { InspectionId = qi1.InspectionId, DefectType = "Cosmetic", Severity = "Minor", Description = "Surface scratch on panel", CorrectiveAction = "Rework", Status = "Open", CompanyId = companyId, ReportedAt = DateTime.UtcNow }
                );

                // Production Costs
                _context.ProductionCosts.AddRange(
                    new Models.ProductionCost { WorkOrderId = wo1.WorkOrderId, CostType = "Material", Description = "Raw Aluminum", Amount = 1500m, CompanyId = companyId },
                    new Models.ProductionCost { WorkOrderId = wo1.WorkOrderId, CostType = "Labor", Description = "Assembly Line 1", Amount = 800m, CompanyId = companyId },
                    new Models.ProductionCost { WorkOrderId = wo2.WorkOrderId, CostType = "Overhead", Description = "Electricity", Amount = 500m, CompanyId = companyId }
                );

                await _context.SaveChangesAsync();
            }

            return Ok("Seeding completed successfully for Admin Jetro's company!");
        }

        private int? GetCompanyId() =>
            int.TryParse(User.FindFirst("CompanyId")?.Value, out var id) ? id : null;
    }
}
