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
    [Route("api/work-orders")]
    [Authorize]
    public class WorkOrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<ProductionHub> _productionHub;

        public WorkOrdersController(ApplicationDbContext context, IHubContext<ProductionHub> productionHub)
        {
            _context = context;
            _productionHub = productionHub;
        }

        [HttpGet]
        public async Task<ActionResult<List<WorkOrderDto>>> GetAll()
        {
            var companyId = GetCompanyId();
            var orders = await _context.WorkOrders
                .Where(wo => wo.CompanyId == companyId)
                .Select(wo => new WorkOrderDto
                {
                    WorkOrderId = wo.WorkOrderId,
                    WorkOrderNumber = wo.WorkOrderNumber,
                    ProductName = wo.ProductName,
                    Quantity = wo.Quantity,
                    CompletedQuantity = wo.CompletedQuantity,
                    Status = wo.Status,
                    Priority = wo.Priority,
                    StartDate = wo.StartDate,
                    DueDate = wo.DueDate,
                    CompletionDate = wo.CompletionDate,
                    AssignedTo = wo.AssignedTo,
                    ScheduleId = wo.ScheduleId,
                    CompanyId = wo.CompanyId
                }).ToListAsync();

            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<WorkOrderDto>> GetById(int id)
        {
            var wo = await _context.WorkOrders
                .FirstOrDefaultAsync(w => w.WorkOrderId == id && w.CompanyId == GetCompanyId());

            if (wo == null) return NotFound();

            return Ok(new WorkOrderDto
            {
                WorkOrderId = wo.WorkOrderId,
                WorkOrderNumber = wo.WorkOrderNumber,
                ProductName = wo.ProductName,
                Quantity = wo.Quantity,
                CompletedQuantity = wo.CompletedQuantity,
                Status = wo.Status,
                Priority = wo.Priority,
                StartDate = wo.StartDate,
                DueDate = wo.DueDate,
                CompletionDate = wo.CompletionDate,
                AssignedTo = wo.AssignedTo,
                ScheduleId = wo.ScheduleId,
                CompanyId = wo.CompanyId
            });
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin,Admin,Production Planner,Shop Floor Supervisor")]
        public async Task<ActionResult<WorkOrderDto>> Create([FromBody] CreateWorkOrderDto dto)
        {
            var companyId = GetCompanyId() ?? 1;

            // Generate work order number
            var count = await _context.WorkOrders.CountAsync(wo => wo.CompanyId == companyId);
            var woNumber = $"WO-{DateTime.UtcNow:yyyy}-{(count + 1):D3}";

            var workOrder = new WorkOrder
            {
                WorkOrderNumber = woNumber,
                ProductName = dto.ProductName,
                Quantity = dto.Quantity,
                Status = "Pending",
                Priority = dto.Priority ?? "Medium",
                StartDate = dto.StartDate,
                DueDate = dto.DueDate,
                AssignedTo = dto.AssignedTo,
                ScheduleId = dto.ScheduleId,
                Notes = dto.Notes,
                CompanyId = companyId,
                CreatedAt = DateTime.UtcNow
            };

            _context.WorkOrders.Add(workOrder);
            await _context.SaveChangesAsync();

            await _productionHub.Clients.Group($"company-{companyId}")
                .SendAsync("ReceiveWorkOrderUpdate", new { action = "Created", workOrder.WorkOrderId, workOrder.WorkOrderNumber });

            return CreatedAtAction(nameof(GetById), new { id = workOrder.WorkOrderId }, new WorkOrderDto
            {
                WorkOrderId = workOrder.WorkOrderId,
                WorkOrderNumber = workOrder.WorkOrderNumber,
                ProductName = workOrder.ProductName,
                Quantity = workOrder.Quantity,
                Status = workOrder.Status,
                Priority = workOrder.Priority,
                CompanyId = workOrder.CompanyId
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Super Admin,Admin,Production Planner,Shop Floor Supervisor")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateWorkOrderDto dto)
        {
            var wo = await _context.WorkOrders
                .FirstOrDefaultAsync(w => w.WorkOrderId == id && w.CompanyId == GetCompanyId());

            if (wo == null) return NotFound();

            wo.ProductName = dto.ProductName;
            wo.Quantity = dto.Quantity;
            wo.Priority = dto.Priority;
            wo.StartDate = dto.StartDate;
            wo.DueDate = dto.DueDate;
            wo.AssignedTo = dto.AssignedTo;
            wo.Notes = dto.Notes;
            wo.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _productionHub.Clients.Group($"company-{wo.CompanyId}")
                .SendAsync("ReceiveWorkOrderUpdate", new { action = "Updated", wo.WorkOrderId });

            return NoContent();
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Super Admin,Admin,Shop Floor Supervisor")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            var wo = await _context.WorkOrders
                .FirstOrDefaultAsync(w => w.WorkOrderId == id && w.CompanyId == GetCompanyId());

            if (wo == null) return NotFound();

            wo.Status = status;
            if (status == "Completed") wo.CompletionDate = DateTime.UtcNow;
            wo.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _productionHub.Clients.Group($"company-{wo.CompanyId}")
                .SendAsync("ReceiveWorkOrderUpdate", new { action = "StatusChanged", wo.WorkOrderId, status });

            return Ok();
        }

        [HttpPatch("{id}/progress")]
        [Authorize(Roles = "Super Admin,Admin,Shop Floor Supervisor")]
        public async Task<IActionResult> UpdateProgress(int id, [FromBody] int completedQuantity)
        {
            var wo = await _context.WorkOrders
                .FirstOrDefaultAsync(w => w.WorkOrderId == id && w.CompanyId == GetCompanyId());

            if (wo == null) return NotFound();

            wo.CompletedQuantity = completedQuantity;
            if (completedQuantity >= wo.Quantity)
            {
                wo.Status = "Completed";
                wo.CompletionDate = DateTime.UtcNow;
            }
            else if (completedQuantity > 0)
            {
                wo.Status = "InProgress";
            }
            wo.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _productionHub.Clients.Group($"company-{wo.CompanyId}")
                .SendAsync("ReceiveWorkOrderUpdate", new { action = "ProgressUpdated", wo.WorkOrderId, completedQuantity });

            return Ok();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Super Admin,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var wo = await _context.WorkOrders
                .FirstOrDefaultAsync(w => w.WorkOrderId == id && w.CompanyId == GetCompanyId());
            if (wo == null) return NotFound();

            _context.WorkOrders.Remove(wo);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private int? GetCompanyId() =>
            int.TryParse(User.FindFirst("CompanyId")?.Value, out var id) ? id : null;
    }
}
