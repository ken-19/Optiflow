using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Casan_IT15_Project.Data;
using Casan_IT15_Project.DTOs;
using Casan_IT15_Project.Models;

namespace Casan_IT15_Project.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InventoryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InventoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<InventoryItemDto>>> GetAll([FromQuery] bool includeArchived = false)
        {
            var companyId = GetCompanyId();
            var query = _context.InventoryItems
                .Where(i => i.CompanyId == companyId);

            if (!includeArchived)
            {
                query = query.Where(i => i.IsActive);
            }

            var items = await query
                .Include(i => i.Material)
                .Select(i => new InventoryItemDto
                {
                    InventoryId = i.InventoryId,
                    MaterialId = i.MaterialId,
                    MaterialName = i.Material.MaterialName,
                    MaterialCode = i.Material.MaterialCode,
                    QuantityOnHand = i.QuantityOnHand,
                    QuantityReserved = i.QuantityReserved,
                    QuantityAvailable = i.QuantityOnHand - i.QuantityReserved,
                    WarehouseLocation = i.WarehouseLocation,
                    BatchNumber = i.BatchNumber,
                    IsActive = i.IsActive
                }).ToListAsync();

            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<InventoryItemDto>> GetById(int id)
        {
            var item = await _context.InventoryItems
                .Include(i => i.Material)
                .FirstOrDefaultAsync(i => i.InventoryId == id && i.CompanyId == GetCompanyId());

            if (item == null) return NotFound();

            return Ok(new InventoryItemDto
            {
                InventoryId = item.InventoryId,
                MaterialId = item.MaterialId,
                MaterialName = item.Material.MaterialName,
                MaterialCode = item.Material.MaterialCode,
                QuantityOnHand = item.QuantityOnHand,
                QuantityReserved = item.QuantityReserved,
                QuantityAvailable = item.QuantityOnHand - item.QuantityReserved,
                WarehouseLocation = item.WarehouseLocation,
                BatchNumber = item.BatchNumber
            });
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin,Admin,Inventory Manager")]
        public async Task<ActionResult> Create([FromBody] CreateInventoryItemDto dto)
        {
            var companyId = GetCompanyId() ?? 1;
            var item = new InventoryItem
            {
                MaterialId = dto.MaterialId,
                QuantityOnHand = dto.QuantityOnHand,
                WarehouseLocation = dto.WarehouseLocation,
                BatchNumber = dto.BatchNumber,
                CompanyId = companyId,
                CreatedAt = DateTime.UtcNow
            };

            _context.InventoryItems.Add(item);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = item.InventoryId }, item);
        }

        [HttpPatch("{id}/adjust")]
        [Authorize(Roles = "Super Admin,Admin,Inventory Manager")]
        public async Task<IActionResult> AdjustStock(int id, [FromBody] int quantityChange)
        {
            var item = await _context.InventoryItems
                .FirstOrDefaultAsync(i => i.InventoryId == id && i.CompanyId == GetCompanyId());

            if (item == null) return NotFound();

            item.QuantityOnHand += quantityChange;
            item.UpdatedAt = DateTime.UtcNow;
            item.LastRestockedAt = quantityChange > 0 ? DateTime.UtcNow : item.LastRestockedAt;

            await _context.SaveChangesAsync();
            return Ok(new { item.QuantityOnHand });
        }

        [HttpGet("materials")]
        public async Task<ActionResult<List<MaterialDto>>> GetMaterials()
        {
            var companyId = GetCompanyId();
            var materials = await _context.Materials
                .Where(m => m.CompanyId == companyId && m.IsActive)
                .Select(m => new MaterialDto
                {
                    MaterialId = m.MaterialId,
                    MaterialCode = m.MaterialCode,
                    MaterialName = m.MaterialName,
                    Description = m.Description,
                    UnitOfMeasure = m.UnitOfMeasure,
                    UnitCost = m.UnitCost,
                    ReorderLevel = m.ReorderLevel,
                    SupplierName = m.SupplierName
                }).ToListAsync();

            return Ok(materials);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Super Admin,Admin")]
        public async Task<IActionResult> Archive(int id)
        {
            var item = await _context.InventoryItems
                .FirstOrDefaultAsync(i => i.InventoryId == id && i.CompanyId == GetCompanyId());
            if (item == null) return NotFound();

            item.IsActive = false;
            item.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id}/unarchive")]
        [Authorize(Roles = "Super Admin,Admin")]
        public async Task<IActionResult> Unarchive(int id)
        {
            var item = await _context.InventoryItems
                .FirstOrDefaultAsync(i => i.InventoryId == id && i.CompanyId == GetCompanyId());
            if (item == null) return NotFound();

            item.IsActive = true;
            item.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Item restored" });
        }

        private int? GetCompanyId() =>
            int.TryParse(User.FindFirst("CompanyId")?.Value, out var id) ? id : null;
    }
}
