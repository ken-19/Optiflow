using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Casan_IT15_Project.Data;
using Casan_IT15_Project.DTOs;
using Casan_IT15_Project.Models;

namespace Casan_IT15_Project.Controllers.Api
{
    [ApiController]
    [Route("api/production-costs")]
    [Authorize]
    public class CostingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CostingController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<ProductionCostDto>>> GetAll()
        {
            var companyId = GetCompanyId();
            var costs = await _context.ProductionCosts
                .Where(pc => pc.CompanyId == companyId)
                .Include(pc => pc.WorkOrder)
                .Select(pc => new ProductionCostDto
                {
                    CostId = pc.CostId,
                    WorkOrderId = pc.WorkOrderId,
                    WorkOrderNumber = pc.WorkOrder.WorkOrderNumber,
                    CostType = pc.CostType,
                    Description = pc.Description,
                    Amount = pc.Amount,
                    Currency = pc.Currency,
                    IncurredDate = pc.IncurredDate
                }).ToListAsync();

            return Ok(costs);
        }

        [HttpGet("summary")]
        public async Task<ActionResult> GetSummary()
        {
            var companyId = GetCompanyId();
            var costs = await _context.ProductionCosts
                .Where(pc => pc.CompanyId == companyId)
                .GroupBy(pc => pc.CostType)
                .Select(g => new { CostType = g.Key, Total = g.Sum(x => x.Amount) })
                .ToListAsync();

            var total = costs.Sum(c => c.Total);
            return Ok(new { costs, total });
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin,Admin,Cost Accountant")]
        public async Task<ActionResult> Create([FromBody] CreateProductionCostDto dto)
        {
            var companyId = GetCompanyId() ?? 1;
            var cost = new ProductionCost
            {
                WorkOrderId = dto.WorkOrderId,
                CostType = dto.CostType,
                Description = dto.Description,
                Amount = dto.Amount,
                IncurredDate = dto.IncurredDate ?? DateTime.UtcNow,
                CompanyId = companyId,
                CreatedAt = DateTime.UtcNow
            };

            _context.ProductionCosts.Add(cost);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAll), new { id = cost.CostId }, cost);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Super Admin,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var cost = await _context.ProductionCosts
                .FirstOrDefaultAsync(pc => pc.CostId == id && pc.CompanyId == GetCompanyId());
            if (cost == null) return NotFound();

            _context.ProductionCosts.Remove(cost);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private int? GetCompanyId() =>
            int.TryParse(User.FindFirst("CompanyId")?.Value, out var id) ? id : null;
    }
}
