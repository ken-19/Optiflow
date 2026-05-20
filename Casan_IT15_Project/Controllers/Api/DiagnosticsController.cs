using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Casan_IT15_Project.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Casan_IT15_Project.Controllers.Api
{
    [ApiController]
    [Route("api/diagnostics")]
    [AllowAnonymous]
    public class DiagnosticsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DiagnosticsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetStatus()
        {
            var logs = new List<string>();
            try
            {
                logs.Add($"Database provider: {_context.Database.ProviderName}");
                logs.Add($"Can connect: {await _context.Database.CanConnectAsync()}");

                var companiesCount = await _context.Companies.CountAsync();
                logs.Add($"Companies: {companiesCount}");
                foreach (var c in await _context.Companies.ToListAsync())
                {
                    logs.Add($" - CompanyId: {c.CompanyId}, Name: {c.CompanyName}, Plan: {c.SubscriptionPlan}");
                }

                logs.Add($"Users: {await _context.Users.CountAsync()}");
                foreach (var u in await _context.Users.ToListAsync())
                {
                    logs.Add($" - UserId: {u.UserId}, Username: {u.Username}, Email: {u.Email}, CompanyId: {u.CompanyId}");
                }

                logs.Add($"Materials: {await _context.Materials.CountAsync()}");
                logs.Add($"InventoryItems: {await _context.InventoryItems.CountAsync()}");
                logs.Add($"ProductionSchedules: {await _context.ProductionSchedules.CountAsync()}");
                logs.Add($"WorkOrders: {await _context.WorkOrders.CountAsync()}");
                logs.Add($"QualityInspections: {await _context.QualityInspections.CountAsync()}");
                logs.Add($"Defects: {await _context.Defects.CountAsync()}");
                logs.Add($"ProductionCosts: {await _context.ProductionCosts.CountAsync()}");
                logs.Add($"MrpRecords: {await _context.MrpRecords.CountAsync()}");
                logs.Add($"Backups: {await _context.Backups.CountAsync()}");

                return Ok(new { Success = true, Logs = logs });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Error = ex.ToString(), Logs = logs });
            }
        }

        [HttpGet("seed-jetro")]
        public async Task<IActionResult> SeedJetro()
        {
            var logs = new List<string>();
            try
            {
                // Ensure Company 2 exists
                var jetroCompany = await _context.Companies.FirstOrDefaultAsync(c => c.CompanyId == 2);
                if (jetroCompany == null)
                {
                    jetroCompany = new Models.Company
                    {
                        CompanyId = 2,
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
                    logs.Add("Created Jetro Manufacturing (CompanyId = 2).");
                }
                else
                {
                    logs.Add("Jetro Manufacturing (CompanyId = 2) already exists.");
                }

                // Ensure materials exist
                var mat1 = await _context.Materials.FirstOrDefaultAsync(m => m.CompanyId == 2 && m.MaterialCode == "MAT-JETRO-001");
                if (mat1 == null)
                {
                    mat1 = new Models.Material { MaterialCode = "MAT-JETRO-001", MaterialName = "Aluminum Sheet", Description = "High grade aluminum", UnitOfMeasure = "kg", UnitCost = 15.50m, ReorderLevel = 100, MinimumOrderQty = 500, IsActive = true, CreatedAt = DateTime.UtcNow, CompanyId = 2 };
                    _context.Materials.Add(mat1);
                    logs.Add("Staged material 1.");
                }
                var mat2 = await _context.Materials.FirstOrDefaultAsync(m => m.CompanyId == 2 && m.MaterialCode == "MAT-JETRO-002");
                if (mat2 == null)
                {
                    mat2 = new Models.Material { MaterialCode = "MAT-JETRO-002", MaterialName = "Steel Bolt M8", Description = "Standard steel bolt", UnitOfMeasure = "pcs", UnitCost = 0.50m, ReorderLevel = 500, MinimumOrderQty = 1000, IsActive = true, CreatedAt = DateTime.UtcNow, CompanyId = 2 };
                    _context.Materials.Add(mat2);
                    logs.Add("Staged material 2.");
                }
                var mat3 = await _context.Materials.FirstOrDefaultAsync(m => m.CompanyId == 2 && m.MaterialCode == "MAT-JETRO-003");
                if (mat3 == null)
                {
                    mat3 = new Models.Material { MaterialCode = "MAT-JETRO-003", MaterialName = "Copper Wire", Description = "Insulated copper wire", UnitOfMeasure = "m", UnitCost = 3.20m, ReorderLevel = 200, MinimumOrderQty = 1000, IsActive = true, CreatedAt = DateTime.UtcNow, CompanyId = 2 };
                    _context.Materials.Add(mat3);
                    logs.Add("Staged material 3.");
                }
                await _context.SaveChangesAsync();
                logs.Add("Saved Materials.");

                // Ensure inventory items exist
                var inv1 = await _context.InventoryItems.FirstOrDefaultAsync(i => i.CompanyId == 2 && i.MaterialId == mat1.MaterialId);
                if (inv1 == null)
                {
                    inv1 = new Models.InventoryItem { MaterialId = mat1.MaterialId, QuantityOnHand = 500, QuantityReserved = 0, WarehouseLocation = "Warehouse A", LastRestockedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, CompanyId = 2 };
                    _context.InventoryItems.Add(inv1);
                    logs.Add("Staged inventory 1.");
                }
                var inv2 = await _context.InventoryItems.FirstOrDefaultAsync(i => i.CompanyId == 2 && i.MaterialId == mat2.MaterialId);
                if (inv2 == null)
                {
                    inv2 = new Models.InventoryItem { MaterialId = mat2.MaterialId, QuantityOnHand = 2000, QuantityReserved = 100, WarehouseLocation = "Warehouse B", LastRestockedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, CompanyId = 2 };
                    _context.InventoryItems.Add(inv2);
                    logs.Add("Staged inventory 2.");
                }
                var inv3 = await _context.InventoryItems.FirstOrDefaultAsync(i => i.CompanyId == 2 && i.MaterialId == mat3.MaterialId);
                if (inv3 == null)
                {
                    inv3 = new Models.InventoryItem { MaterialId = mat3.MaterialId, QuantityOnHand = 50, QuantityReserved = 0, WarehouseLocation = "Warehouse A", LastRestockedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, CompanyId = 2 };
                    _context.InventoryItems.Add(inv3);
                    logs.Add("Staged inventory 3.");
                }
                await _context.SaveChangesAsync();
                logs.Add("Saved InventoryItems.");

                // Ensure MRP
                var mrp1 = await _context.MrpRecords.FirstOrDefaultAsync(m => m.CompanyId == 2 && m.MaterialId == mat3.MaterialId);
                if (mrp1 == null)
                {
                    mrp1 = new Models.MrpRecord { MaterialId = mat3.MaterialId, RequiredQuantity = 300, AvailableQuantity = 50, Status = "Shortage", RequiredDate = DateTime.UtcNow.AddDays(7), CalculatedAt = DateTime.UtcNow, CompanyId = 2 };
                    _context.MrpRecords.Add(mrp1);
                    logs.Add("Staged MRP 1.");
                }
                var mrp2 = await _context.MrpRecords.FirstOrDefaultAsync(m => m.CompanyId == 2 && m.MaterialId == mat1.MaterialId);
                if (mrp2 == null)
                {
                    mrp2 = new Models.MrpRecord { MaterialId = mat1.MaterialId, RequiredQuantity = 150, AvailableQuantity = 500, Status = "Sufficient", RequiredDate = DateTime.UtcNow.AddDays(14), CalculatedAt = DateTime.UtcNow, CompanyId = 2 };
                    _context.MrpRecords.Add(mrp2);
                    logs.Add("Staged MRP 2.");
                }
                await _context.SaveChangesAsync();
                logs.Add("Saved MrpRecords.");

                // Ensure production schedules
                var sched1 = await _context.ProductionSchedules.FirstOrDefaultAsync(ps => ps.CompanyId == 2 && ps.ProductName == "Engine Block Alpha JETRO");
                if (sched1 == null)
                {
                    sched1 = new Models.ProductionSchedule { ProductName = "Engine Block Alpha JETRO", PlannedQuantity = 100, StartDate = DateTime.UtcNow.AddDays(-2), EndDate = DateTime.UtcNow.AddDays(5), Status = "InProgress", Priority = "High", CreatedAt = DateTime.UtcNow, CompanyId = 2 };
                    _context.ProductionSchedules.Add(sched1);
                    logs.Add("Staged production schedule 1.");
                }
                var sched2 = await _context.ProductionSchedules.FirstOrDefaultAsync(ps => ps.CompanyId == 2 && ps.ProductName == "Transmission Unit B JETRO");
                if (sched2 == null)
                {
                    sched2 = new Models.ProductionSchedule { ProductName = "Transmission Unit B JETRO", PlannedQuantity = 50, StartDate = DateTime.UtcNow.AddDays(-10), EndDate = DateTime.UtcNow.AddDays(-1), Status = "Completed", Priority = "Medium", CreatedAt = DateTime.UtcNow, CompanyId = 2 };
                    _context.ProductionSchedules.Add(sched2);
                    logs.Add("Staged production schedule 2.");
                }
                await _context.SaveChangesAsync();
                logs.Add("Saved ProductionSchedules.");

                // Ensure work orders
                var wo1 = await _context.WorkOrders.FirstOrDefaultAsync(w => w.CompanyId == 2 && w.WorkOrderNumber == "WO-JETRO-001");
                if (wo1 == null)
                {
                    wo1 = new Models.WorkOrder { WorkOrderNumber = "WO-JETRO-001", ProductName = "Engine Block Alpha JETRO", Quantity = 100, CompletedQuantity = 45, Status = "InProgress", Priority = "High", StartDate = DateTime.UtcNow.AddDays(-2), DueDate = DateTime.UtcNow.AddDays(5), AssignedTo = "supervisor_jetro", Notes = "Urgent batch", CreatedAt = DateTime.UtcNow, ScheduleId = sched1.ScheduleId, CompanyId = 2 };
                    _context.WorkOrders.Add(wo1);
                    logs.Add("Staged work order 1.");
                }
                var wo2 = await _context.WorkOrders.FirstOrDefaultAsync(w => w.CompanyId == 2 && w.WorkOrderNumber == "WO-JETRO-002");
                if (wo2 == null)
                {
                    wo2 = new Models.WorkOrder { WorkOrderNumber = "WO-JETRO-002", ProductName = "Transmission Unit B JETRO", Quantity = 50, CompletedQuantity = 50, Status = "Completed", Priority = "Medium", StartDate = DateTime.UtcNow.AddDays(-10), DueDate = DateTime.UtcNow.AddDays(-1), AssignedTo = "planner_jetro", Notes = "Standard run", CreatedAt = DateTime.UtcNow, ScheduleId = sched2.ScheduleId, CompanyId = 2 };
                    _context.WorkOrders.Add(wo2);
                    logs.Add("Staged work order 2.");
                }
                var wo3 = await _context.WorkOrders.FirstOrDefaultAsync(w => w.CompanyId == 2 && w.WorkOrderNumber == "WO-JETRO-003");
                if (wo3 == null)
                {
                    wo3 = new Models.WorkOrder { WorkOrderNumber = "WO-JETRO-003", ProductName = "Chassis Frame JETRO", Quantity = 200, CompletedQuantity = 0, Status = "Pending", Priority = "Low", StartDate = DateTime.UtcNow.AddDays(2), DueDate = DateTime.UtcNow.AddDays(15), AssignedTo = "supervisor_jetro", Notes = "Awaiting materials", CreatedAt = DateTime.UtcNow, ScheduleId = null, CompanyId = 2 };
                    _context.WorkOrders.Add(wo3);
                    logs.Add("Staged work order 3.");
                }
                await _context.SaveChangesAsync();
                logs.Add("Saved WorkOrders.");

                // Ensure quality inspections
                var qi1 = await _context.QualityInspections.FirstOrDefaultAsync(q => q.CompanyId == 2 && q.WorkOrderId == wo1.WorkOrderId);
                if (qi1 == null)
                {
                    qi1 = new Models.QualityInspection { WorkOrderId = wo1.WorkOrderId, InspectorName = "quality_jetro", InspectionDate = DateTime.UtcNow, SampleSize = 10, PassedCount = 8, FailedCount = 2, Result = "Fail", Notes = "Surface scratch", CreatedAt = DateTime.UtcNow, CompanyId = 2 };
                    _context.QualityInspections.Add(qi1);
                    logs.Add("Staged quality inspection 1.");
                }
                var qi2 = await _context.QualityInspections.FirstOrDefaultAsync(q => q.CompanyId == 2 && q.WorkOrderId == wo2.WorkOrderId);
                if (qi2 == null)
                {
                    qi2 = new Models.QualityInspection { WorkOrderId = wo2.WorkOrderId, InspectorName = "quality_jetro", InspectionDate = DateTime.UtcNow.AddDays(-2), SampleSize = 5, PassedCount = 5, FailedCount = 0, Result = "Pass", Notes = "All good", CreatedAt = DateTime.UtcNow.AddDays(-2), CompanyId = 2 };
                    _context.QualityInspections.Add(qi2);
                    logs.Add("Staged quality inspection 2.");
                }
                await _context.SaveChangesAsync();
                logs.Add("Saved QualityInspections.");

                // Ensure defects
                var defect1 = await _context.Defects.FirstOrDefaultAsync(d => d.CompanyId == 2 && d.InspectionId == qi1.InspectionId);
                if (defect1 == null)
                {
                    defect1 = new Models.Defect { InspectionId = qi1.InspectionId, DefectType = "Cosmetic", Severity = "Minor", DefectCount = 2, Description = "Surface scratch on panel", CorrectiveAction = "Rework", Status = "Open", ReportedAt = DateTime.UtcNow, CompanyId = 2 };
                    _context.Defects.Add(defect1);
                    logs.Add("Staged defect 1.");
                }
                await _context.SaveChangesAsync();
                logs.Add("Saved Defects.");

                // Ensure production costs
                var cost1 = await _context.ProductionCosts.FirstOrDefaultAsync(c => c.CompanyId == 2 && c.WorkOrderId == wo1.WorkOrderId && c.CostType == "Material");
                if (cost1 == null)
                {
                    cost1 = new Models.ProductionCost { WorkOrderId = wo1.WorkOrderId, CostType = "Material", Description = "Raw Aluminum", Amount = 1500.00m, Currency = "PHP", IncurredDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, CompanyId = 2 };
                    _context.ProductionCosts.Add(cost1);
                    logs.Add("Staged cost 1.");
                }
                var cost2 = await _context.ProductionCosts.FirstOrDefaultAsync(c => c.CompanyId == 2 && c.WorkOrderId == wo1.WorkOrderId && c.CostType == "Labor");
                if (cost2 == null)
                {
                    cost2 = new Models.ProductionCost { WorkOrderId = wo1.WorkOrderId, CostType = "Labor", Description = "Assembly Line 1", Amount = 800.00m, Currency = "PHP", IncurredDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, CompanyId = 2 };
                    _context.ProductionCosts.Add(cost2);
                    logs.Add("Staged cost 2.");
                }
                var cost3 = await _context.ProductionCosts.FirstOrDefaultAsync(c => c.CompanyId == 2 && c.WorkOrderId == wo2.WorkOrderId && c.CostType == "Overhead");
                if (cost3 == null)
                {
                    cost3 = new Models.ProductionCost { WorkOrderId = wo2.WorkOrderId, CostType = "Overhead", Description = "Electricity", Amount = 500.00m, Currency = "PHP", IncurredDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, CompanyId = 2 };
                    _context.ProductionCosts.Add(cost3);
                    logs.Add("Staged cost 3.");
                }
                await _context.SaveChangesAsync();
                logs.Add("Saved ProductionCosts.");

                return Ok(new { Success = true, Logs = logs });
            }
            catch (Exception ex)
            {
                return Ok(new { Success = false, Error = ex.ToString(), Logs = logs });
            }
        }
    }
}
