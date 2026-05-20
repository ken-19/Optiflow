using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Casan_IT15_Project.Data;
using Casan_IT15_Project.DTOs;

namespace Casan_IT15_Project.Controllers.Api
{
    /// <summary>
    /// Dashboard API — aggregated data for charts and KPIs.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get dashboard summary — accessible via GET /api/dashboard or /api/dashboard/summary
        /// </summary>
        [HttpGet]
        [HttpGet("summary")]
        public async Task<ActionResult> GetSummary()
        {
            var companyId = GetCompanyId();

            var totalWorkOrders = await _context.WorkOrders.CountAsync(wo => wo.CompanyId == companyId);
            var activeWorkOrders = await _context.WorkOrders.CountAsync(wo => wo.CompanyId == companyId && wo.Status == "InProgress");
            var completedWorkOrders = await _context.WorkOrders.CountAsync(wo => wo.CompanyId == companyId && wo.Status == "Completed");
            var pendingWorkOrders = await _context.WorkOrders.CountAsync(wo => wo.CompanyId == companyId && wo.Status == "Pending");
            var totalMaterials = await _context.Materials.CountAsync(m => m.CompanyId == companyId);

            var lowStockItems = await _context.InventoryItems
                .Include(i => i.Material)
                .Where(i => i.CompanyId == companyId && i.QuantityOnHand <= i.Material.ReorderLevel)
                .CountAsync();

            var totalInspections = await _context.QualityInspections.CountAsync(qi => qi.CompanyId == companyId);
            var openDefects = await _context.Defects.CountAsync(d => d.CompanyId == companyId && d.Status == "Open");
            var totalCost = await _context.ProductionCosts
                .Where(pc => pc.CompanyId == companyId)
                .SumAsync(pc => (decimal?)pc.Amount) ?? 0;

            var activeSchedules = await _context.ProductionSchedules
                .CountAsync(ps => ps.CompanyId == companyId && ps.Status == "InProgress");

            // Work orders grouped by status — return as dictionary for frontend charts
            var woByStatusList = await _context.WorkOrders
                .Where(wo => wo.CompanyId == companyId)
                .GroupBy(wo => wo.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();
            var workOrderStatusDistribution = woByStatusList.ToDictionary(x => x.Status, x => x.Count);

            // Monthly costs for the last 6 months — return as dictionary
            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
            var rawMonthlyCosts = await _context.ProductionCosts
                .Where(pc => pc.CompanyId == companyId && pc.IncurredDate >= sixMonthsAgo)
                .GroupBy(pc => new { pc.IncurredDate.Year, pc.IncurredDate.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.Amount) })
                .ToListAsync();

            var monthlyCostsList = rawMonthlyCosts
                .Select(x => new { Month = $"{x.Year}-{x.Month:D2}", x.Total })
                .OrderBy(x => x.Month)
                .ToList();
            var monthlyCosts = monthlyCostsList.ToDictionary(x => x.Month, x => x.Total);

            // Defects by severity — return as dictionary
            var defectsList = await _context.Defects
                .Where(d => d.CompanyId == companyId)
                .GroupBy(d => d.Severity)
                .Select(g => new { Severity = g.Key, Count = g.Count() })
                .ToListAsync();
            var defectSeverity = defectsList.ToDictionary(x => x.Severity, x => x.Count);

            // Defect rate
            var totalInspected = await _context.QualityInspections
                .Where(qi => qi.CompanyId == companyId)
                .SumAsync(qi => (int?)qi.SampleSize) ?? 0;
            var totalFailed = await _context.QualityInspections
                .Where(qi => qi.CompanyId == companyId)
                .SumAsync(qi => (int?)qi.FailedCount) ?? 0;
            var defectRate = totalInspected > 0 ? Math.Round((double)totalFailed / totalInspected * 100, 1) : 0;

            return Ok(new
            {
                totalWorkOrders,
                activeWorkOrders,
                completedWorkOrders,
                pendingWorkOrders,
                totalMaterials,
                lowStockItems,
                totalInspections,
                openDefects,
                totalCost,
                activeSchedules,
                defectRate,
                workOrderStatusDistribution,
                monthlyCosts,
                defectSeverity
            });
        }

        private int? GetCompanyId() =>
            int.TryParse(User.FindFirst("CompanyId")?.Value, out var id) ? id : null;
    }
}
