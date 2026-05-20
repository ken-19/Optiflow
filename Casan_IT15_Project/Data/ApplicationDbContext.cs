using Microsoft.EntityFrameworkCore;
using Casan_IT15_Project.Models;

namespace Casan_IT15_Project.Data
{
    /// <summary>
    /// Main database context for the OptiFlow ERP-MES system.
    /// Contains all DbSets and relationship configurations.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ===== Auth & RBAC =====
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<Permission> Permissions { get; set; } = null!;
        public DbSet<UserRole> UserRoles { get; set; } = null!;
        public DbSet<RolePermission> RolePermissions { get; set; } = null!;

        // ===== Multi-tenant =====
        public DbSet<Company> Companies { get; set; } = null!;

        // ===== Production =====
        public DbSet<ProductionSchedule> ProductionSchedules { get; set; } = null!;
        public DbSet<WorkOrder> WorkOrders { get; set; } = null!;

        // ===== Materials & Inventory =====
        public DbSet<Material> Materials { get; set; } = null!;
        public DbSet<InventoryItem> InventoryItems { get; set; } = null!;
        public DbSet<MrpRecord> MrpRecords { get; set; } = null!;

        // ===== Quality =====
        public DbSet<QualityInspection> QualityInspections { get; set; } = null!;
        public DbSet<Defect> Defects { get; set; } = null!;

        // ===== Costing =====
        public DbSet<ProductionCost> ProductionCosts { get; set; } = null!;

        // ===== Reports & System =====
        public DbSet<Report> Reports { get; set; } = null!;
        public DbSet<SystemLog> SystemLogs { get; set; } = null!;
        public DbSet<Backup> Backups { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== Relationship Configurations =====

            // User → Company (many-to-one)
            modelBuilder.Entity<User>()
                .HasOne(u => u.Company)
                .WithMany(c => c.Users)
                .HasForeignKey(u => u.CompanyId)
                .OnDelete(DeleteBehavior.SetNull);

            // UserRole (User ↔ Role many-to-many)
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // RolePermission (Role ↔ Permission many-to-many)
            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            // ProductionSchedule → Company
            modelBuilder.Entity<ProductionSchedule>()
                .HasOne(ps => ps.Company)
                .WithMany(c => c.ProductionSchedules)
                .HasForeignKey(ps => ps.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            // WorkOrder → ProductionSchedule
            modelBuilder.Entity<WorkOrder>()
                .HasOne(wo => wo.ProductionSchedule)
                .WithMany(ps => ps.WorkOrders)
                .HasForeignKey(wo => wo.ScheduleId)
                .OnDelete(DeleteBehavior.SetNull);

            // WorkOrder → Company
            modelBuilder.Entity<WorkOrder>()
                .HasOne(wo => wo.Company)
                .WithMany(c => c.WorkOrders)
                .HasForeignKey(wo => wo.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            // Material → Company
            modelBuilder.Entity<Material>()
                .HasOne(m => m.Company)
                .WithMany(c => c.Materials)
                .HasForeignKey(m => m.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            // InventoryItem → Material
            modelBuilder.Entity<InventoryItem>()
                .HasOne(i => i.Material)
                .WithMany(m => m.InventoryItems)
                .HasForeignKey(i => i.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);

            // InventoryItem → Company
            modelBuilder.Entity<InventoryItem>()
                .HasOne(i => i.Company)
                .WithMany(c => c.InventoryItems)
                .HasForeignKey(i => i.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);

            // MrpRecord → Material
            modelBuilder.Entity<MrpRecord>()
                .HasOne(m => m.Material)
                .WithMany(mat => mat.MrpRecords)
                .HasForeignKey(m => m.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);

            // MrpRecord → WorkOrder
            modelBuilder.Entity<MrpRecord>()
                .HasOne(m => m.WorkOrder)
                .WithMany(wo => wo.MrpRecords)
                .HasForeignKey(m => m.WorkOrderId)
                .OnDelete(DeleteBehavior.SetNull);

            // QualityInspection → WorkOrder
            modelBuilder.Entity<QualityInspection>()
                .HasOne(qi => qi.WorkOrder)
                .WithMany(wo => wo.QualityInspections)
                .HasForeignKey(qi => qi.WorkOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Defect → QualityInspection
            modelBuilder.Entity<Defect>()
                .HasOne(d => d.QualityInspection)
                .WithMany(qi => qi.Defects)
                .HasForeignKey(d => d.InspectionId)
                .OnDelete(DeleteBehavior.Cascade);

            // ProductionCost → WorkOrder
            modelBuilder.Entity<ProductionCost>()
                .HasOne(pc => pc.WorkOrder)
                .WithMany(wo => wo.ProductionCosts)
                .HasForeignKey(pc => pc.WorkOrderId)
                .OnDelete(DeleteBehavior.Restrict);

            // ===== Decimal precision =====
            modelBuilder.Entity<Material>()
                .Property(m => m.UnitCost).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<ProductionCost>()
                .Property(pc => pc.Amount).HasColumnType("decimal(18,2)");

            // ===== Unique constraints =====
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username).IsUnique();
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<Role>()
                .HasIndex(r => r.RoleName).IsUnique();
            // Per-company uniqueness for multi-tenant isolation
            modelBuilder.Entity<Material>()
                .HasIndex(m => new { m.CompanyId, m.MaterialCode }).IsUnique();
            modelBuilder.Entity<WorkOrder>()
                .HasIndex(wo => new { wo.CompanyId, wo.WorkOrderNumber }).IsUnique();

            // ===== Ignore computed properties =====
            modelBuilder.Entity<InventoryItem>()
                .Ignore(i => i.QuantityAvailable);
            modelBuilder.Entity<MrpRecord>()
                .Ignore(m => m.ShortageQuantity);

            // ===== SEED DATA =====
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // --- Companies ---
            modelBuilder.Entity<Company>().HasData(
                new Company
                {
                    CompanyId = 1,
                    CompanyName = "OptiFlow Manufacturing Inc.",
                    Address = "123 Industrial Ave, Manila, Philippines",
                    ContactEmail = "admin@optiflow.com",
                    ContactPhone = "+63-912-345-6789",
                    Industry = "Manufacturing",
                    IsActive = true,
                    SubscriptionPlan = "None",
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Company
                {
                    CompanyId = 2,
                    CompanyName = "Jetro Manufacturing",
                    Address = "456 Production Blvd, Cebu, Philippines",
                    ContactEmail = "admin_jetro@gmail.com",
                    ContactPhone = "+63-917-123-4567",
                    Industry = "Manufacturing",
                    IsActive = true,
                    SubscriptionPlan = "Premium Plan",
                    SubscriptionExpiry = new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // --- Roles (8 roles as specified) ---
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Super Admin", Description = "Full system access across all companies", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Role { RoleId = 2, RoleName = "Admin", Description = "Company owner with full company access", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Role { RoleId = 3, RoleName = "Production Planner", Description = "Manages production schedules and planning", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Role { RoleId = 4, RoleName = "Inventory Manager", Description = "Manages inventory and materials", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Role { RoleId = 5, RoleName = "Cost Accountant", Description = "Manages production costing and budgets", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Role { RoleId = 6, RoleName = "Shop Floor Supervisor", Description = "Manages work orders on the shop floor", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Role { RoleId = 7, RoleName = "Quality Control Inspector", Description = "Performs quality inspections and defect tracking", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Role { RoleId = 8, RoleName = "Plant Manager", Description = "View-only access to reports and dashboards", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );

            // --- Permissions ---
            modelBuilder.Entity<Permission>().HasData(
                // User Management
                new Permission { PermissionId = 1, PermissionName = "Users.View", Module = "Users", Description = "View user list" },
                new Permission { PermissionId = 2, PermissionName = "Users.Create", Module = "Users", Description = "Create new users" },
                new Permission { PermissionId = 3, PermissionName = "Users.Edit", Module = "Users", Description = "Edit users" },
                new Permission { PermissionId = 4, PermissionName = "Users.Delete", Module = "Users", Description = "Delete users" },
                // Production
                new Permission { PermissionId = 5, PermissionName = "Production.View", Module = "Production", Description = "View production schedules" },
                new Permission { PermissionId = 6, PermissionName = "Production.Create", Module = "Production", Description = "Create production schedules" },
                new Permission { PermissionId = 7, PermissionName = "Production.Edit", Module = "Production", Description = "Edit production schedules" },
                new Permission { PermissionId = 8, PermissionName = "Production.Delete", Module = "Production", Description = "Delete production schedules" },
                // Work Orders
                new Permission { PermissionId = 9, PermissionName = "WorkOrders.View", Module = "WorkOrders", Description = "View work orders" },
                new Permission { PermissionId = 10, PermissionName = "WorkOrders.Create", Module = "WorkOrders", Description = "Create work orders" },
                new Permission { PermissionId = 11, PermissionName = "WorkOrders.Edit", Module = "WorkOrders", Description = "Edit work orders" },
                new Permission { PermissionId = 12, PermissionName = "WorkOrders.Delete", Module = "WorkOrders", Description = "Delete work orders" },
                // Inventory
                new Permission { PermissionId = 13, PermissionName = "Inventory.View", Module = "Inventory", Description = "View inventory" },
                new Permission { PermissionId = 14, PermissionName = "Inventory.Create", Module = "Inventory", Description = "Add inventory items" },
                new Permission { PermissionId = 15, PermissionName = "Inventory.Edit", Module = "Inventory", Description = "Edit inventory" },
                new Permission { PermissionId = 16, PermissionName = "Inventory.Delete", Module = "Inventory", Description = "Delete inventory items" },
                // MRP
                new Permission { PermissionId = 17, PermissionName = "MRP.View", Module = "MRP", Description = "View MRP records" },
                new Permission { PermissionId = 18, PermissionName = "MRP.Calculate", Module = "MRP", Description = "Run MRP calculations" },
                // Quality
                new Permission { PermissionId = 19, PermissionName = "Quality.View", Module = "Quality", Description = "View inspections" },
                new Permission { PermissionId = 20, PermissionName = "Quality.Create", Module = "Quality", Description = "Create inspections" },
                new Permission { PermissionId = 21, PermissionName = "Quality.Edit", Module = "Quality", Description = "Edit inspections" },
                // Costing
                new Permission { PermissionId = 22, PermissionName = "Costing.View", Module = "Costing", Description = "View costs" },
                new Permission { PermissionId = 23, PermissionName = "Costing.Create", Module = "Costing", Description = "Add cost entries" },
                new Permission { PermissionId = 24, PermissionName = "Costing.Edit", Module = "Costing", Description = "Edit cost entries" },
                // Reports
                new Permission { PermissionId = 25, PermissionName = "Reports.View", Module = "Reports", Description = "View reports" },
                new Permission { PermissionId = 26, PermissionName = "Reports.Generate", Module = "Reports", Description = "Generate reports" },
                // System
                new Permission { PermissionId = 27, PermissionName = "System.Logs", Module = "System", Description = "View system logs" },
                new Permission { PermissionId = 28, PermissionName = "System.Backup", Module = "System", Description = "Create backups" },
                new Permission { PermissionId = 29, PermissionName = "System.Config", Module = "System", Description = "System configuration" }
            );

            // --- Super Admin user (password: Admin@123) — global scope, no company ---
            // BCrypt hash of "Admin@123"
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    Username = "superadmin",
                    Email = "superadmin@optiflow.com",
                    PasswordHash = "$2a$11$zUnFAZJQItyVsfm/pILx9uA6PRE95ACf9oaQ6rtSp47k6UgLZIRZe",
                    FirstName = "Super",
                    LastName = "Admin",
                    IsActive = true,
                    CompanyId = null, // Super Admin is global — not tied to any company
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new User
                {
                    UserId = 2,
                    Username = "admin_jetro@gmail.com",
                    Email = "admin_jetro@gmail.com",
                    PasswordHash = "$2a$11$zUnFAZJQItyVsfm/pILx9uA6PRE95ACf9oaQ6rtSp47k6UgLZIRZe",
                    FirstName = "Admin",
                    LastName = "Jetro",
                    IsActive = true,
                    CompanyId = 2,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new User
                {
                    UserId = 3,
                    Username = "planner_jetro@gmail.com",
                    Email = "planner_jetro@gmail.com",
                    PasswordHash = "$2a$11$zUnFAZJQItyVsfm/pILx9uA6PRE95ACf9oaQ6rtSp47k6UgLZIRZe",
                    FirstName = "Planner",
                    LastName = "Jetro",
                    IsActive = true,
                    CompanyId = 2,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new User
                {
                    UserId = 4,
                    Username = "inventory_jetro@gmail.com",
                    Email = "inventory_jetro@gmail.com",
                    PasswordHash = "$2a$11$zUnFAZJQItyVsfm/pILx9uA6PRE95ACf9oaQ6rtSp47k6UgLZIRZe",
                    FirstName = "Inventory",
                    LastName = "Jetro",
                    IsActive = true,
                    CompanyId = 2,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new User
                {
                    UserId = 5,
                    Username = "cost_jetro@gmail.com",
                    Email = "cost_jetro@gmail.com",
                    PasswordHash = "$2a$11$zUnFAZJQItyVsfm/pILx9uA6PRE95ACf9oaQ6rtSp47k6UgLZIRZe",
                    FirstName = "Cost",
                    LastName = "Jetro",
                    IsActive = true,
                    CompanyId = 2,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new User
                {
                    UserId = 6,
                    Username = "supervisor_jetro@gmail.com",
                    Email = "supervisor_jetro@gmail.com",
                    PasswordHash = "$2a$11$zUnFAZJQItyVsfm/pILx9uA6PRE95ACf9oaQ6rtSp47k6UgLZIRZe",
                    FirstName = "Supervisor",
                    LastName = "Jetro",
                    IsActive = true,
                    CompanyId = 2,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new User
                {
                    UserId = 7,
                    Username = "quality_jetro@gmail.com",
                    Email = "quality_jetro@gmail.com",
                    PasswordHash = "$2a$11$zUnFAZJQItyVsfm/pILx9uA6PRE95ACf9oaQ6rtSp47k6UgLZIRZe",
                    FirstName = "Quality",
                    LastName = "Jetro",
                    IsActive = true,
                    CompanyId = 2,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new User
                {
                    UserId = 8,
                    Username = "manager_jetro@gmail.com",
                    Email = "manager_jetro@gmail.com",
                    PasswordHash = "$2a$11$zUnFAZJQItyVsfm/pILx9uA6PRE95ACf9oaQ6rtSp47k6UgLZIRZe",
                    FirstName = "Manager",
                    LastName = "Jetro",
                    IsActive = true,
                    CompanyId = 2,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // --- Assign Roles ---
            modelBuilder.Entity<UserRole>().HasData(
                new UserRole
                {
                    UserRoleId = 1,
                    UserId = 1,
                    RoleId = 1, // Super Admin
                    AssignedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new UserRole
                {
                    UserRoleId = 2,
                    UserId = 2,
                    RoleId = 2, // Admin
                    AssignedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new UserRole
                {
                    UserRoleId = 3,
                    UserId = 3,
                    RoleId = 3, // Production Planner
                    AssignedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new UserRole
                {
                    UserRoleId = 4,
                    UserId = 4,
                    RoleId = 4, // Inventory Manager
                    AssignedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new UserRole
                {
                    UserRoleId = 5,
                    UserId = 5,
                    RoleId = 5, // Cost Accountant
                    AssignedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new UserRole
                {
                    UserRoleId = 6,
                    UserId = 6,
                    RoleId = 6, // Shop Floor Supervisor
                    AssignedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new UserRole
                {
                    UserRoleId = 7,
                    UserId = 7,
                    RoleId = 7, // Quality Control Inspector
                    AssignedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new UserRole
                {
                    UserRoleId = 8,
                    UserId = 8,
                    RoleId = 8, // Plant Manager
                    AssignedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            // --- Give Super Admin all permissions ---
            for (int i = 1; i <= 29; i++)
            {
                modelBuilder.Entity<RolePermission>().HasData(new RolePermission
                {
                    RolePermissionId = i,
                    RoleId = 1,
                    PermissionId = i
                });
            }

            // --- Give Admin role permissions (all except System.Backup and System.Config) ---
            for (int i = 1; i <= 27; i++)
            {
                modelBuilder.Entity<RolePermission>().HasData(new RolePermission
                {
                    RolePermissionId = 29 + i,
                    RoleId = 2,
                    PermissionId = i
                });
            }

            // --- Sample Materials ---
            modelBuilder.Entity<Material>().HasData(
                new Material { MaterialId = 1, MaterialCode = "MAT-001", MaterialName = "Steel Sheet 4x8", UnitOfMeasure = "Sheet", UnitCost = 1500.00m, ReorderLevel = 50, MinimumOrderQty = 10, SupplierName = "PhilSteel Corp", CompanyId = 1, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Material { MaterialId = 2, MaterialCode = "MAT-002", MaterialName = "Aluminum Bar 2m", UnitOfMeasure = "Piece", UnitCost = 850.00m, ReorderLevel = 100, MinimumOrderQty = 25, SupplierName = "MetalWorks PH", CompanyId = 1, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Material { MaterialId = 3, MaterialCode = "MAT-003", MaterialName = "Industrial Paint - Blue", UnitOfMeasure = "Liter", UnitCost = 320.00m, ReorderLevel = 30, MinimumOrderQty = 5, SupplierName = "ColorTech Inc", CompanyId = 1, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Material { MaterialId = 4, MaterialCode = "MAT-004", MaterialName = "Welding Wire 1.2mm", UnitOfMeasure = "Roll", UnitCost = 2200.00m, ReorderLevel = 20, MinimumOrderQty = 5, SupplierName = "WeldSupply Co", CompanyId = 1, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Material { MaterialId = 5, MaterialCode = "MAT-005", MaterialName = "Rubber Gasket Set", UnitOfMeasure = "Set", UnitCost = 180.00m, ReorderLevel = 200, MinimumOrderQty = 50, SupplierName = "RubberTech PH", CompanyId = 1, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );

            // --- Sample Production Schedule ---
            modelBuilder.Entity<ProductionSchedule>().HasData(
                new ProductionSchedule { ScheduleId = 1, ProductName = "Industrial Pump Assembly", PlannedQuantity = 100, StartDate = new DateTime(2026, 4, 1), EndDate = new DateTime(2026, 4, 30), Status = "InProgress", Priority = "High", CompanyId = 1, CreatedByUserId = 1, CreatedAt = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc) },
                new ProductionSchedule { ScheduleId = 2, ProductName = "Conveyor Belt Module", PlannedQuantity = 50, StartDate = new DateTime(2026, 5, 1), EndDate = new DateTime(2026, 5, 31), Status = "Planned", Priority = "Medium", CompanyId = 1, CreatedByUserId = 1, CreatedAt = new DateTime(2026, 1, 15, 0, 0, 0, DateTimeKind.Utc) }
            );

            // --- Sample Work Orders ---
            modelBuilder.Entity<WorkOrder>().HasData(
                new WorkOrder { WorkOrderId = 1, WorkOrderNumber = "WO-2026-001", ProductName = "Industrial Pump Assembly", Quantity = 50, CompletedQuantity = 30, Status = "InProgress", Priority = "High", StartDate = new DateTime(2026, 4, 1), DueDate = new DateTime(2026, 4, 15), ScheduleId = 1, CompanyId = 1, CreatedAt = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc) },
                new WorkOrder { WorkOrderId = 2, WorkOrderNumber = "WO-2026-002", ProductName = "Industrial Pump Assembly", Quantity = 50, CompletedQuantity = 0, Status = "Pending", Priority = "High", StartDate = new DateTime(2026, 4, 15), DueDate = new DateTime(2026, 4, 30), ScheduleId = 1, CompanyId = 1, CreatedAt = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc) }
            );

            // --- Sample Inventory ---
            modelBuilder.Entity<InventoryItem>().HasData(
                new InventoryItem { InventoryId = 1, MaterialId = 1, QuantityOnHand = 200, QuantityReserved = 50, WarehouseLocation = "Warehouse A-1", BatchNumber = "BATCH-2026-001", CompanyId = 1, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new InventoryItem { InventoryId = 2, MaterialId = 2, QuantityOnHand = 350, QuantityReserved = 100, WarehouseLocation = "Warehouse A-2", BatchNumber = "BATCH-2026-002", CompanyId = 1, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new InventoryItem { InventoryId = 3, MaterialId = 3, QuantityOnHand = 45, QuantityReserved = 10, WarehouseLocation = "Warehouse B-1", BatchNumber = "BATCH-2026-003", CompanyId = 1, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );
        }
    }
}
