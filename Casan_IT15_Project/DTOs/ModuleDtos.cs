namespace Casan_IT15_Project.DTOs
{
    // ===== User DTOs =====
    public class UserDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public bool IsActive { get; set; }
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public List<string> Roles { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }

    public class CreateUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int? CompanyId { get; set; }
        public int? RoleId { get; set; }
    }

    public class UpdateUserDto
    {
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public bool? IsActive { get; set; }
    }

    // ===== Production DTOs =====
    public class ProductionScheduleDto
    {
        public int ScheduleId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int PlannedQuantity { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Priority { get; set; }
        public string? Notes { get; set; }
        public int CompanyId { get; set; }
        public int WorkOrderCount { get; set; }
    }

    public class CreateProductionScheduleDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int PlannedQuantity { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Priority { get; set; }
        public string? Notes { get; set; }
    }

    // ===== Work Order DTOs =====
    public class WorkOrderDto
    {
        public int WorkOrderId { get; set; }
        public string WorkOrderNumber { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int CompletedQuantity { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Priority { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public string? AssignedTo { get; set; }
        public int? ScheduleId { get; set; }
        public int CompanyId { get; set; }
    }

    public class CreateWorkOrderDto
    {
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string? Priority { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string? AssignedTo { get; set; }
        public int? ScheduleId { get; set; }
        public string? Notes { get; set; }
    }

    // ===== Inventory DTOs =====
    public class InventoryItemDto
    {
        public int InventoryId { get; set; }
        public int MaterialId { get; set; }
        public string MaterialName { get; set; } = string.Empty;
        public string MaterialCode { get; set; } = string.Empty;
        public int QuantityOnHand { get; set; }
        public int QuantityReserved { get; set; }
        public int QuantityAvailable { get; set; }
        public string? WarehouseLocation { get; set; }
        public string? BatchNumber { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateInventoryItemDto
    {
        public int MaterialId { get; set; }
        public int QuantityOnHand { get; set; }
        public string? WarehouseLocation { get; set; }
        public string? BatchNumber { get; set; }
    }

    // ===== Material DTOs =====
    public class MaterialDto
    {
        public int MaterialId { get; set; }
        public string MaterialCode { get; set; } = string.Empty;
        public string MaterialName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? UnitOfMeasure { get; set; }
        public decimal UnitCost { get; set; }
        public int ReorderLevel { get; set; }
        public string? SupplierName { get; set; }
    }

    // ===== Quality DTOs =====
    public class QualityInspectionDto
    {
        public int InspectionId { get; set; }
        public int WorkOrderId { get; set; }
        public string WorkOrderNumber { get; set; } = string.Empty;
        public string InspectorName { get; set; } = string.Empty;
        public DateTime InspectionDate { get; set; }
        public int SampleSize { get; set; }
        public int PassedCount { get; set; }
        public int FailedCount { get; set; }
        public string Result { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public int DefectCount { get; set; }
    }

    public class CreateInspectionDto
    {
        public int WorkOrderId { get; set; }
        public string InspectorName { get; set; } = string.Empty;
        public int SampleSize { get; set; }
        public int PassedCount { get; set; }
        public int FailedCount { get; set; }
        public string Result { get; set; } = "Pass";
        public string? Notes { get; set; }
    }

    public class DefectDto
    {
        public int DefectId { get; set; }
        public int InspectionId { get; set; }
        public string DefectType { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public int DefectCount { get; set; }
        public string? Description { get; set; }
        public string? CorrectiveAction { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime ReportedAt { get; set; }
    }

    public class CreateDefectDto
    {
        public int InspectionId { get; set; }
        public string DefectType { get; set; } = string.Empty;
        public string Severity { get; set; } = "Medium";
        public int DefectCount { get; set; } = 1;
        public string? Description { get; set; }
        public string? CorrectiveAction { get; set; }
    }

    // ===== Costing DTOs =====
    public class ProductionCostDto
    {
        public int CostId { get; set; }
        public int WorkOrderId { get; set; }
        public string WorkOrderNumber { get; set; } = string.Empty;
        public string CostType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "PHP";
        public DateTime IncurredDate { get; set; }
    }

    public class CreateProductionCostDto
    {
        public int WorkOrderId { get; set; }
        public string CostType { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Amount { get; set; }
        public DateTime? IncurredDate { get; set; }
    }

    // ===== Dashboard DTOs =====
    public class DashboardSummaryDto
    {
        public int TotalWorkOrders { get; set; }
        public int ActiveWorkOrders { get; set; }
        public int CompletedWorkOrders { get; set; }
        public int TotalMaterials { get; set; }
        public int LowStockItems { get; set; }
        public int TotalInspections { get; set; }
        public int OpenDefects { get; set; }
        public decimal TotalProductionCost { get; set; }
        public List<WorkOrderStatusCount> WorkOrdersByStatus { get; set; } = new();
        public List<MonthlyCostData> MonthlyCosts { get; set; } = new();
        public List<DefectSeverityCount> DefectsBySeverity { get; set; } = new();
    }

    public class WorkOrderStatusCount
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class MonthlyCostData
    {
        public string Month { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }

    public class DefectSeverityCount
    {
        public string Severity { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
