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
    public class QualityController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public QualityController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ===== Inspections =====

        [HttpGet("inspections")]
        public async Task<ActionResult<List<QualityInspectionDto>>> GetInspections()
        {
            var companyId = GetCompanyId();
            var inspections = await _context.QualityInspections
                .Where(qi => qi.CompanyId == companyId)
                .Include(qi => qi.WorkOrder)
                .Include(qi => qi.Defects)
                .Select(qi => new QualityInspectionDto
                {
                    InspectionId = qi.InspectionId,
                    WorkOrderId = qi.WorkOrderId,
                    WorkOrderNumber = qi.WorkOrder.WorkOrderNumber,
                    InspectorName = qi.InspectorName,
                    InspectionDate = qi.InspectionDate,
                    SampleSize = qi.SampleSize,
                    PassedCount = qi.PassedCount,
                    FailedCount = qi.FailedCount,
                    Result = qi.Result,
                    Notes = qi.Notes,
                    DefectCount = qi.Defects.Count
                }).ToListAsync();

            return Ok(inspections);
        }

        [HttpPost("inspections")]
        [Authorize(Roles = "Super Admin,Admin,Quality Control Inspector")]
        public async Task<ActionResult> CreateInspection([FromBody] CreateInspectionDto dto)
        {
            var companyId = GetCompanyId() ?? 1;
            var inspection = new QualityInspection
            {
                WorkOrderId = dto.WorkOrderId,
                InspectorName = dto.InspectorName,
                SampleSize = dto.SampleSize,
                PassedCount = dto.PassedCount,
                FailedCount = dto.FailedCount,
                Result = dto.Result,
                Notes = dto.Notes,
                CompanyId = companyId,
                InspectionDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            _context.QualityInspections.Add(inspection);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetInspections), new { id = inspection.InspectionId }, inspection);
        }

        // ===== Defects =====

        [HttpGet("defects")]
        public async Task<ActionResult<List<DefectDto>>> GetDefects()
        {
            var companyId = GetCompanyId();
            var defects = await _context.Defects
                .Where(d => d.CompanyId == companyId)
                .Select(d => new DefectDto
                {
                    DefectId = d.DefectId,
                    InspectionId = d.InspectionId,
                    DefectType = d.DefectType,
                    Severity = d.Severity,
                    DefectCount = d.DefectCount,
                    Description = d.Description,
                    CorrectiveAction = d.CorrectiveAction,
                    Status = d.Status,
                    ReportedAt = d.ReportedAt
                }).ToListAsync();

            return Ok(defects);
        }

        [HttpPost("defects")]
        [Authorize(Roles = "Super Admin,Admin,Quality Control Inspector")]
        public async Task<ActionResult> CreateDefect([FromBody] CreateDefectDto dto)
        {
            var companyId = GetCompanyId() ?? 1;
            var defect = new Defect
            {
                InspectionId = dto.InspectionId,
                DefectType = dto.DefectType,
                Severity = dto.Severity,
                DefectCount = dto.DefectCount,
                Description = dto.Description,
                CorrectiveAction = dto.CorrectiveAction,
                Status = "Open",
                CompanyId = companyId,
                ReportedAt = DateTime.UtcNow
            };

            _context.Defects.Add(defect);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetDefects), new { id = defect.DefectId }, defect);
        }

        [HttpPatch("defects/{id}/resolve")]
        [Authorize(Roles = "Super Admin,Admin,Quality Control Inspector")]
        public async Task<IActionResult> ResolveDefect(int id, [FromBody] string correctiveAction)
        {
            var defect = await _context.Defects
                .FirstOrDefaultAsync(d => d.DefectId == id && d.CompanyId == GetCompanyId());

            if (defect == null) return NotFound();

            defect.Status = "Resolved";
            defect.CorrectiveAction = correctiveAction;
            defect.ResolvedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok();
        }

        private int? GetCompanyId() =>
            int.TryParse(User.FindFirst("CompanyId")?.Value, out var id) ? id : null;
    }
}
