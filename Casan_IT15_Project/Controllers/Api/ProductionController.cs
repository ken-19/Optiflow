using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Casan_IT15_Project.Data;
using Casan_IT15_Project.DTOs;
using Casan_IT15_Project.Hubs;
using Casan_IT15_Project.Models;

namespace Casan_IT15_Project.Controllers.Api
{
    [ApiController]
    [Route("api/production-schedules")]
    [Authorize]
    public class ProductionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ProductionHub> _productionHub;

        public ProductionController(ApplicationDbContext context, IHubContext<ProductionHub> productionHub)
        {
            _context = context;
            _productionHub = productionHub;
        }

        [HttpGet]
        public async Task<ActionResult<List<ProductionScheduleDto>>> GetAll()
        {
            var companyId = GetCompanyId();
            var schedules = await _context.ProductionSchedules
                .Where(ps => ps.CompanyId == companyId)
                .Include(ps => ps.WorkOrders)
                .Select(ps => new ProductionScheduleDto
                {
                    ScheduleId = ps.ScheduleId,
                    ProductName = ps.ProductName,
                    PlannedQuantity = ps.PlannedQuantity,
                    StartDate = ps.StartDate,
                    EndDate = ps.EndDate,
                    Status = ps.Status,
                    Priority = ps.Priority,
                    Notes = ps.Notes,
                    CompanyId = ps.CompanyId,
                    WorkOrderCount = ps.WorkOrders.Count
                }).ToListAsync();

            return Ok(schedules);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductionScheduleDto>> GetById(int id)
        {
            var ps = await _context.ProductionSchedules
                .Include(p => p.WorkOrders)
                .FirstOrDefaultAsync(p => p.ScheduleId == id && p.CompanyId == GetCompanyId());

            if (ps == null) return NotFound();

            return Ok(new ProductionScheduleDto
            {
                ScheduleId = ps.ScheduleId,
                ProductName = ps.ProductName,
                PlannedQuantity = ps.PlannedQuantity,
                StartDate = ps.StartDate,
                EndDate = ps.EndDate,
                Status = ps.Status,
                Priority = ps.Priority,
                Notes = ps.Notes,
                CompanyId = ps.CompanyId,
                WorkOrderCount = ps.WorkOrders.Count
            });
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin,Admin,Production Planner")]
        public async Task<ActionResult<ProductionScheduleDto>> Create([FromBody] CreateProductionScheduleDto dto)
        {
            var companyId = GetCompanyId() ?? 1;
            var userId = GetUserId();

            var schedule = new ProductionSchedule
            {
                ProductName = dto.ProductName,
                PlannedQuantity = dto.PlannedQuantity,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Status = "Planned",
                Priority = dto.Priority ?? "Medium",
                Notes = dto.Notes,
                CompanyId = companyId,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.ProductionSchedules.Add(schedule);
            await _context.SaveChangesAsync();

            // Broadcast real-time update
            await _productionHub.Clients.Group($"company-{companyId}")
                .SendAsync("ReceiveProductionUpdate", new { action = "Created", schedule.ScheduleId, schedule.ProductName });

            return CreatedAtAction(nameof(GetById), new { id = schedule.ScheduleId }, new ProductionScheduleDto
            {
                ScheduleId = schedule.ScheduleId,
                ProductName = schedule.ProductName,
                PlannedQuantity = schedule.PlannedQuantity,
                StartDate = schedule.StartDate,
                EndDate = schedule.EndDate,
                Status = schedule.Status,
                Priority = schedule.Priority,
                CompanyId = schedule.CompanyId
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Super Admin,Admin,Production Planner")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateProductionScheduleDto dto)
        {
            var schedule = await _context.ProductionSchedules
                .FirstOrDefaultAsync(ps => ps.ScheduleId == id && ps.CompanyId == GetCompanyId());

            if (schedule == null) return NotFound();

            schedule.ProductName = dto.ProductName;
            schedule.PlannedQuantity = dto.PlannedQuantity;
            schedule.StartDate = dto.StartDate;
            schedule.EndDate = dto.EndDate;
            schedule.Priority = dto.Priority;
            schedule.Notes = dto.Notes;
            schedule.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _productionHub.Clients.Group($"company-{schedule.CompanyId}")
                .SendAsync("ReceiveProductionUpdate", new { action = "Updated", schedule.ScheduleId });

            return NoContent();
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Super Admin,Admin,Production Planner,Shop Floor Supervisor")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            var schedule = await _context.ProductionSchedules
                .FirstOrDefaultAsync(ps => ps.ScheduleId == id && ps.CompanyId == GetCompanyId());

            if (schedule == null) return NotFound();

            schedule.Status = status;
            schedule.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _productionHub.Clients.Group($"company-{schedule.CompanyId}")
                .SendAsync("ReceiveProductionUpdate", new { action = "StatusChanged", schedule.ScheduleId, status });

            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Super Admin,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var schedule = await _context.ProductionSchedules
                .FirstOrDefaultAsync(ps => ps.ScheduleId == id && ps.CompanyId == GetCompanyId());

            if (schedule == null) return NotFound();

            _context.ProductionSchedules.Remove(schedule);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private int? GetCompanyId() =>
            int.TryParse(User.FindFirst("CompanyId")?.Value, out var id) ? id : null;

        private int GetUserId() =>
            int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
    }
}
