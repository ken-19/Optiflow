using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Casan_IT15_Project.Data;
using Casan_IT15_Project.Models;

namespace Casan_IT15_Project.Controllers.Api
{
    /// <summary>
    /// Material Requirements Planning (MRP) controller.
    /// Calculates material needs based on work orders and current inventory.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MrpController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MrpController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all MRP records with calculated shortages.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> GetMrpRecords()
        {
            var companyId = GetCompanyId();

            // Get materials with their inventory and active work order requirements
            var materials = await _context.Materials
                .Where(m => m.CompanyId == companyId && m.IsActive)
                .Include(m => m.InventoryItems)
                .ToListAsync();

            var activeWorkOrders = await _context.WorkOrders
                .Where(wo => wo.CompanyId == companyId && wo.Status != "Completed" && wo.Status != "Cancelled")
                .ToListAsync();

            var mrpRecords = new List<object>();
            int id = 1;

            foreach (var material in materials)
            {
                var totalOnHand = material.InventoryItems
                    .Where(i => i.CompanyId == companyId)
                    .Sum(i => i.QuantityOnHand);

                var totalReserved = material.InventoryItems
                    .Where(i => i.CompanyId == companyId)
                    .Sum(i => i.QuantityReserved);

                var availableQuantity = totalOnHand - totalReserved;

                // Estimate required quantity based on active work orders
                // In a real system this would use a Bill of Materials (BOM)
                var requiredQuantity = activeWorkOrders.Count > 0
                    ? material.ReorderLevel * 2  // Simplified calculation
                    : material.ReorderLevel;

                var shortfall = Math.Max(0, requiredQuantity - availableQuantity);

                mrpRecords.Add(new
                {
                    mrpId = id++,
                    materialId = material.MaterialId,
                    materialName = material.MaterialName,
                    materialCode = material.MaterialCode,
                    requiredQuantity,
                    availableQuantity,
                    shortfall,
                    reorderLevel = material.ReorderLevel,
                    minimumOrderQty = material.MinimumOrderQty,
                    supplierName = material.SupplierName,
                    unitCost = material.UnitCost,
                    estimatedCost = shortfall * material.UnitCost,
                    status = shortfall > 0 ? "Shortage" : "Sufficient",
                    workOrderNumber = activeWorkOrders.FirstOrDefault()?.WorkOrderNumber ?? "N/A"
                });
            }

            return Ok(mrpRecords);
        }

        /// <summary>
        /// Run MRP calculation and return summary.
        /// </summary>
        [HttpPost("calculate")]
        [Authorize(Roles = "Super Admin,Admin,Production Planner,Inventory Manager")]
        public async Task<ActionResult> CalculateMrp()
        {
            var companyId = GetCompanyId();

            var materials = await _context.Materials
                .Where(m => m.CompanyId == companyId && m.IsActive)
                .Include(m => m.InventoryItems)
                .ToListAsync();

            int totalMaterials = materials.Count;
            int shortages = 0;
            decimal totalEstimatedCost = 0;

            foreach (var material in materials)
            {
                var available = material.InventoryItems
                    .Where(i => i.CompanyId == companyId)
                    .Sum(i => i.QuantityOnHand - i.QuantityReserved);

                if (available < material.ReorderLevel)
                {
                    shortages++;
                    var shortfall = material.ReorderLevel - available;
                    totalEstimatedCost += shortfall * material.UnitCost;
                }
            }

            return Ok(new
            {
                totalMaterials,
                sufficient = totalMaterials - shortages,
                shortages,
                totalEstimatedCost,
                calculatedAt = DateTime.UtcNow
            });
        }

        private int? GetCompanyId() =>
            int.TryParse(User.FindFirst("CompanyId")?.Value, out var id) ? id : null;
    }
}
