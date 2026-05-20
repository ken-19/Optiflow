DECLARE @CompanyId INT = 3;
DECLARE @Hash NVARCHAR(MAX) = '$2a$11$lY7E.sAv.Z5kZGMwsySv1OeUNUazcP7HxDBf9EKYhtel4PvvMVxyW';
DECLARE @Now DATETIME = GETUTCDATE();

-- Insert Users
INSERT INTO Users (Username, Email, PasswordHash, FirstName, LastName, CompanyId, IsActive, CreatedAt)
VALUES 
('planner_jetro', 'planner_jetro@gmail.com', @Hash, 'Test', 'Production Planner', @CompanyId, 1, @Now),
('inventory_jetro', 'inventory_jetro@gmail.com', @Hash, 'Test', 'Inventory Manager', @CompanyId, 1, @Now),
('cost_jetro', 'cost_jetro@gmail.com', @Hash, 'Test', 'Cost Accountant', @CompanyId, 1, @Now),
('supervisor_jetro', 'supervisor_jetro@gmail.com', @Hash, 'Test', 'Shop Floor Supervisor', @CompanyId, 1, @Now),
('quality_jetro', 'quality_jetro@gmail.com', @Hash, 'Test', 'Quality Control Inspector', @CompanyId, 1, @Now),
('manager_jetro', 'manager_jetro@gmail.com', @Hash, 'Test', 'Plant Manager', @CompanyId, 1, @Now);

-- Assign Roles
INSERT INTO UserRoles (UserId, RoleId, AssignedAt)
SELECT UserId, 3, @Now FROM Users WHERE Username = 'planner_jetro';
INSERT INTO UserRoles (UserId, RoleId, AssignedAt)
SELECT UserId, 4, @Now FROM Users WHERE Username = 'inventory_jetro';
INSERT INTO UserRoles (UserId, RoleId, AssignedAt)
SELECT UserId, 5, @Now FROM Users WHERE Username = 'cost_jetro';
INSERT INTO UserRoles (UserId, RoleId, AssignedAt)
SELECT UserId, 6, @Now FROM Users WHERE Username = 'supervisor_jetro';
INSERT INTO UserRoles (UserId, RoleId, AssignedAt)
SELECT UserId, 7, @Now FROM Users WHERE Username = 'quality_jetro';
INSERT INTO UserRoles (UserId, RoleId, AssignedAt)
SELECT UserId, 8, @Now FROM Users WHERE Username = 'manager_jetro';

-- Insert Materials
INSERT INTO Materials (MaterialCode, MaterialName, Description, UnitOfMeasure, UnitCost, ReorderLevel, MinimumOrderQty, IsActive, CreatedAt, CompanyId)
VALUES 
('MAT-001', 'Aluminum Sheet', 'High grade aluminum', 'kg', 15.50, 100, 500, 1, @Now, @CompanyId),
('MAT-002', 'Steel Bolt M8', 'Standard steel bolt', 'pcs', 0.50, 500, 1000, 1, @Now, @CompanyId),
('MAT-003', 'Copper Wire', 'Insulated copper wire', 'm', 3.20, 200, 1000, 1, @Now, @CompanyId);

-- Get Material IDs
DECLARE @Mat1 INT = (SELECT MaterialId FROM Materials WHERE MaterialCode = 'MAT-001' AND CompanyId = @CompanyId);
DECLARE @Mat2 INT = (SELECT MaterialId FROM Materials WHERE MaterialCode = 'MAT-002' AND CompanyId = @CompanyId);
DECLARE @Mat3 INT = (SELECT MaterialId FROM Materials WHERE MaterialCode = 'MAT-003' AND CompanyId = @CompanyId);

-- Insert Inventory
INSERT INTO InventoryItems (MaterialId, QuantityOnHand, QuantityReserved, WarehouseLocation, LastRestockedAt, CreatedAt, CompanyId)
VALUES 
(@Mat1, 500, 0, 'Warehouse A', @Now, @Now, @CompanyId),
(@Mat2, 2000, 100, 'Warehouse B', @Now, @Now, @CompanyId),
(@Mat3, 50, 0, 'Warehouse A', @Now, @Now, @CompanyId);

-- Insert MRP Records
INSERT INTO MrpRecords (MaterialId, RequiredQuantity, AvailableQuantity, Status, RequiredDate, CalculatedAt, CompanyId)
VALUES 
(@Mat3, 300, 50, 'Shortage', DATEADD(day, 7, @Now), @Now, @CompanyId),
(@Mat1, 150, 500, 'Sufficient', DATEADD(day, 14, @Now), @Now, @CompanyId);

-- Insert Production Schedules
INSERT INTO ProductionSchedules (ProductName, PlannedQuantity, StartDate, EndDate, Status, Priority, CreatedAt, CompanyId)
VALUES 
('Engine Block Alpha', 100, DATEADD(day, -2, @Now), DATEADD(day, 5, @Now), 'InProgress', 'High', @Now, @CompanyId),
('Transmission Unit B', 50, DATEADD(day, -10, @Now), DATEADD(day, -1, @Now), 'Completed', 'Medium', @Now, @CompanyId);

DECLARE @Sched1 INT = (SELECT ScheduleId FROM ProductionSchedules WHERE ProductName = 'Engine Block Alpha' AND CompanyId = @CompanyId);
DECLARE @Sched2 INT = (SELECT ScheduleId FROM ProductionSchedules WHERE ProductName = 'Transmission Unit B' AND CompanyId = @CompanyId);

-- Insert Work Orders
INSERT INTO WorkOrders (WorkOrderNumber, ProductName, Quantity, CompletedQuantity, Status, Priority, StartDate, DueDate, AssignedTo, Notes, CreatedAt, ScheduleId, CompanyId)
VALUES 
('WO-2026-001', 'Engine Block Alpha', 100, 45, 'InProgress', 'High', DATEADD(day, -2, @Now), DATEADD(day, 5, @Now), 'supervisor_jetro', 'Urgent batch', @Now, @Sched1, @CompanyId),
('WO-2026-002', 'Transmission Unit B', 50, 50, 'Completed', 'Medium', DATEADD(day, -10, @Now), DATEADD(day, -1, @Now), 'planner_jetro', 'Standard run', @Now, @Sched2, @CompanyId),
('WO-2026-003', 'Chassis Frame', 200, 0, 'Pending', 'Low', DATEADD(day, 2, @Now), DATEADD(day, 15, @Now), 'supervisor_jetro', 'Awaiting materials', @Now, NULL, @CompanyId);

DECLARE @Wo1 INT = (SELECT WorkOrderId FROM WorkOrders WHERE WorkOrderNumber = 'WO-2026-001' AND CompanyId = @CompanyId);
DECLARE @Wo2 INT = (SELECT WorkOrderId FROM WorkOrders WHERE WorkOrderNumber = 'WO-2026-002' AND CompanyId = @CompanyId);

-- Insert Quality Inspections
INSERT INTO QualityInspections (WorkOrderId, InspectorName, InspectionDate, SampleSize, PassedCount, FailedCount, Result, Notes, CreatedAt, CompanyId)
VALUES 
(@Wo1, 'quality_jetro', @Now, 10, 8, 2, 'Fail', 'Surface scratch', @Now, @CompanyId),
(@Wo2, 'quality_jetro', DATEADD(day, -2, @Now), 5, 5, 0, 'Pass', 'All good', DATEADD(day, -2, @Now), @CompanyId);

DECLARE @Insp1 INT = (SELECT InspectionId FROM QualityInspections WHERE WorkOrderId = @Wo1 AND CompanyId = @CompanyId);

-- Insert Defects
INSERT INTO Defects (InspectionId, DefectType, Severity, DefectCount, Description, CorrectiveAction, Status, ReportedAt, CompanyId)
VALUES 
(@Insp1, 'Cosmetic', 'Minor', 2, 'Surface scratch on panel', 'Rework', 'Open', @Now, @CompanyId);

-- Insert Production Costs
INSERT INTO ProductionCosts (WorkOrderId, CostType, Description, Amount, Currency, IncurredDate, CreatedAt, CompanyId)
VALUES 
(@Wo1, 'Material', 'Raw Aluminum', 1500.00, 'PHP', @Now, @Now, @CompanyId),
(@Wo1, 'Labor', 'Assembly Line 1', 800.00, 'PHP', @Now, @Now, @CompanyId),
(@Wo2, 'Overhead', 'Electricity', 500.00, 'PHP', @Now, @Now, @CompanyId);
