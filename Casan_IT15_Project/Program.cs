using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Casan_IT15_Project.Data;
using Casan_IT15_Project.Services;
using Casan_IT15_Project.Hubs;

var builder = WebApplication.CreateBuilder(args);

// ===== Database =====
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";
if (connectionString.Contains("Data Source=") && connectionString.EndsWith(".db"))
{
    // SQLite mode
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(connectionString));
}
else
{
    // SQL Server mode
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }));
}

// ===== JWT Authentication =====
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"] ?? "OptiFlowSuperSecretKey2026!@#$%^&*()ERP-MES-System-IT15";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "OptiFlow",
        ValidAudience = jwtSettings["Audience"] ?? "OptiFlowClient",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };

    // Allow SignalR to receive token from query string
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// ===== Services (DI) =====
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<ISystemLogService, SystemLogService>();
builder.Services.AddScoped<IStripeService, StripeService>();

// ===== Controllers + Views + API =====
builder.Services.AddControllersWithViews();

// ===== SignalR =====
builder.Services.AddSignalR();

// ===== CORS (for React dev server) =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000", "https://optiflowsystem101.runasp.net", "http://32.198.37.105")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ===== Swagger =====
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "OptiFlow ERP-MES API",
        Version = "v1",
        Description = "Web-Based ERP-MES System for Production Optimization"
    });

    // JWT auth in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by your JWT token"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Auto-apply database schema (ensures DB is always ready)
try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (connectionString.Contains(".db"))
        {
            db.Database.EnsureCreated(); // SQLite: create from model + seed
        }
        else
        {
            db.Database.Migrate(); // SQL Server: apply migrations
        }

        // Ensure both companies exist
        var hasCompany1 = db.Companies.Any(c => c.CompanyId == 1);
        var hasCompany2 = db.Companies.Any(c => c.CompanyId == 2);

        if (!hasCompany1 || !hasCompany2)
        {
            if (!connectionString.Contains(".db"))
            {
                db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Companies ON");
            }

            if (!hasCompany1)
            {
                db.Companies.Add(new Casan_IT15_Project.Models.Company { CompanyId = 1, CompanyName = "OptiFlow Manufacturing Inc.", Address = "123 Industrial Ave, Manila, Philippines", ContactEmail = "admin@optiflow.com", ContactPhone = "+63-912-345-6789", Industry = "Manufacturing", IsActive = true, SubscriptionPlan = "None", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) });
            }
            if (!hasCompany2)
            {
                db.Companies.Add(new Casan_IT15_Project.Models.Company { CompanyId = 2, CompanyName = "Jetro Manufacturing", Address = "456 Production Blvd, Cebu, Philippines", ContactEmail = "admin_jetro@gmail.com", ContactPhone = "+63-917-123-4567", Industry = "Manufacturing", IsActive = true, SubscriptionPlan = "Premium Plan", SubscriptionExpiry = new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc), CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) });
            }

            db.SaveChanges();

            if (!connectionString.Contains(".db"))
            {
                db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Companies OFF");
            }
        }

        // Ensure users exist
        var usersToSeed = new List<Casan_IT15_Project.Models.User>
        {
            new Casan_IT15_Project.Models.User { UserId = 1, Username = "superadmin", Email = "superadmin@optiflow.com", PasswordHash = "$2a$11$zUnFAZJQItyVsfm/pILx9uA6PRE95ACf9oaQ6rtSp47k6UgLZIRZe", FirstName = "Super", LastName = "Admin", IsActive = true, CompanyId = null, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Casan_IT15_Project.Models.User { UserId = 2, Username = "admin_jetro@gmail.com", Email = "admin_jetro@gmail.com", PasswordHash = "$2a$11$zUnFAZJQItyVsfm/pILx9uA6PRE95ACf9oaQ6rtSp47k6UgLZIRZe", FirstName = "Admin", LastName = "Jetro", IsActive = true, CompanyId = 2, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Casan_IT15_Project.Models.User { UserId = 3, Username = "planner_jetro@gmail.com", Email = "planner_jetro@gmail.com", PasswordHash = "$2a$11$zUnFAZJQItyVsfm/pILx9uA6PRE95ACf9oaQ6rtSp47k6UgLZIRZe", FirstName = "Planner", LastName = "Jetro", IsActive = true, CompanyId = 2, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Casan_IT15_Project.Models.User { UserId = 4, Username = "inventory_jetro@gmail.com", Email = "inventory_jetro@gmail.com", PasswordHash = "$2a$11$zUnFAZJQItyVsfm/pILx9uA6PRE95ACf9oaQ6rtSp47k6UgLZIRZe", FirstName = "Inventory", LastName = "Jetro", IsActive = true, CompanyId = 2, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Casan_IT15_Project.Models.User { UserId = 5, Username = "cost_jetro@gmail.com", Email = "cost_jetro@gmail.com", PasswordHash = "$2a$11$zUnFAZJQItyVsfm/pILx9uA6PRE95ACf9oaQ6rtSp47k6UgLZIRZe", FirstName = "Cost", LastName = "Jetro", IsActive = true, CompanyId = 2, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Casan_IT15_Project.Models.User { UserId = 6, Username = "supervisor_jetro@gmail.com", Email = "supervisor_jetro@gmail.com", PasswordHash = "$2a$11$zUnFAZJQItyVsfm/pILx9uA6PRE95ACf9oaQ6rtSp47k6UgLZIRZe", FirstName = "Supervisor", LastName = "Jetro", IsActive = true, CompanyId = 2, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Casan_IT15_Project.Models.User { UserId = 7, Username = "quality_jetro@gmail.com", Email = "quality_jetro@gmail.com", PasswordHash = "$2a$11$zUnFAZJQItyVsfm/pILx9uA6PRE95ACf9oaQ6rtSp47k6UgLZIRZe", FirstName = "Quality", LastName = "Jetro", IsActive = true, CompanyId = 2, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Casan_IT15_Project.Models.User { UserId = 8, Username = "manager_jetro@gmail.com", Email = "manager_jetro@gmail.com", PasswordHash = "$2a$11$zUnFAZJQItyVsfm/pILx9uA6PRE95ACf9oaQ6rtSp47k6UgLZIRZe", FirstName = "Manager", LastName = "Jetro", IsActive = true, CompanyId = 2, CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        };

        var addedUsers = false;
        foreach (var user in usersToSeed)
        {
            if (!db.Users.Any(u => u.UserId == user.UserId || u.Username == user.Username))
            {
                if (!connectionString.Contains(".db") && !addedUsers)
                {
                    db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Users ON");
                }
                db.Users.Add(user);
                addedUsers = true;
            }
        }

        if (addedUsers)
        {
            db.SaveChanges();
            if (!connectionString.Contains(".db"))
            {
                db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Users OFF");
            }
        }

        // Ensure roles exist
        var rolesToSeed = new List<Casan_IT15_Project.Models.Role>
        {
            new Casan_IT15_Project.Models.Role { RoleId = 1, RoleName = "Super Admin", Description = "Full system access across all companies", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Casan_IT15_Project.Models.Role { RoleId = 2, RoleName = "Admin", Description = "Company owner with full company access", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Casan_IT15_Project.Models.Role { RoleId = 3, RoleName = "Production Planner", Description = "Manages production schedules and planning", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Casan_IT15_Project.Models.Role { RoleId = 4, RoleName = "Inventory Manager", Description = "Manages inventory and materials", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Casan_IT15_Project.Models.Role { RoleId = 5, RoleName = "Cost Accountant", Description = "Manages production costing and budgets", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Casan_IT15_Project.Models.Role { RoleId = 6, RoleName = "Shop Floor Supervisor", Description = "Manages work orders on the shop floor", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Casan_IT15_Project.Models.Role { RoleId = 7, RoleName = "Quality Control Inspector", Description = "Performs quality inspections and defect tracking", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Casan_IT15_Project.Models.Role { RoleId = 8, RoleName = "Plant Manager", Description = "View-only access to reports and dashboards", CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        };

        var addedRoles = false;
        foreach (var role in rolesToSeed)
        {
            if (!db.Roles.Any(r => r.RoleId == role.RoleId || r.RoleName == role.RoleName))
            {
                if (!connectionString.Contains(".db") && !addedRoles)
                {
                    db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Roles ON");
                }
                db.Roles.Add(role);
                addedRoles = true;
            }
        }

        if (addedRoles)
        {
            db.SaveChanges();
            if (!connectionString.Contains(".db"))
            {
                db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Roles OFF");
            }
        }

        // Ensure user roles exist
        var userRolesToSeed = new List<Casan_IT15_Project.Models.UserRole>
        {
            new Casan_IT15_Project.Models.UserRole { UserRoleId = 1, UserId = 1, RoleId = 1, AssignedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Casan_IT15_Project.Models.UserRole { UserRoleId = 2, UserId = 2, RoleId = 2, AssignedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Casan_IT15_Project.Models.UserRole { UserRoleId = 3, UserId = 3, RoleId = 3, AssignedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Casan_IT15_Project.Models.UserRole { UserRoleId = 4, UserId = 4, RoleId = 4, AssignedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Casan_IT15_Project.Models.UserRole { UserRoleId = 5, UserId = 5, RoleId = 5, AssignedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Casan_IT15_Project.Models.UserRole { UserRoleId = 6, UserId = 6, RoleId = 6, AssignedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Casan_IT15_Project.Models.UserRole { UserRoleId = 7, UserId = 7, RoleId = 7, AssignedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Casan_IT15_Project.Models.UserRole { UserRoleId = 8, UserId = 8, RoleId = 8, AssignedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        };

        var addedUserRoles = false;
        foreach (var ur in userRolesToSeed)
        {
            if (!db.UserRoles.Any(x => x.UserRoleId == ur.UserRoleId || (x.UserId == ur.UserId && x.RoleId == ur.RoleId)))
            {
                if (!connectionString.Contains(".db") && !addedUserRoles)
                {
                    db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT UserRoles ON");
                }
                db.UserRoles.Add(ur);
                addedUserRoles = true;
            }
        }

        if (addedUserRoles)
        {
            db.SaveChanges();
            if (!connectionString.Contains(".db"))
            {
                db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT UserRoles OFF");
            }
        }

        // Ensure backups exist
        if (!db.Backups.Any())
        {
            var addedBackups = false;
            var backupsToSeed = new List<Casan_IT15_Project.Models.Backup>
            {
                new Casan_IT15_Project.Models.Backup { BackupId = 1, BackupName = "optiflow_backup_20260515_040000", Status = "Completed", CreatedByUserId = 1, CreatedAt = DateTime.UtcNow.AddDays(-5), CompletedAt = DateTime.UtcNow.AddDays(-5), FilePath = "/backups/optiflow_backup_20260515_040000.bak", FileSizeBytes = 254281920L },
                new Casan_IT15_Project.Models.Backup { BackupId = 2, BackupName = "optiflow_backup_20260518_040000", Status = "Completed", CreatedByUserId = 1, CreatedAt = DateTime.UtcNow.AddDays(-2), CompletedAt = DateTime.UtcNow.AddDays(-2), FilePath = "/backups/optiflow_backup_20260518_040000.bak", FileSizeBytes = 258932224L },
                new Casan_IT15_Project.Models.Backup { BackupId = 3, BackupName = "optiflow_backup_20260520_040000", Status = "Completed", CreatedByUserId = 1, CreatedAt = DateTime.UtcNow.AddHours(-6), CompletedAt = DateTime.UtcNow.AddHours(-6), FilePath = "/backups/optiflow_backup_20260520_040000.bak", FileSizeBytes = 262681600L }
            };

            foreach (var b in backupsToSeed)
            {
                if (!connectionString.Contains(".db") && !addedBackups)
                {
                    db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Backups ON");
                }
                db.Backups.Add(b);
                addedBackups = true;
            }

            if (addedBackups)
            {
                db.SaveChanges();
                if (!connectionString.Contains(".db"))
                {
                    db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Backups OFF");
                }
            }
        }
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred while migrating/seeding the database.");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "OptiFlow API v1");
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseRouting();

// CORS must be before Auth
app.UseCors("ReactApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// API routes
app.MapControllerRoute(
    name: "api",
    pattern: "api/{controller}/{action=Index}/{id?}");

// SignalR hubs
app.MapHub<ProductionHub>("/hubs/production");
app.MapHub<NotificationHub>("/hubs/notifications");

// Fallback: Serve React's index.html for all non-API routes (SPA routing)
app.MapFallback(async (context) =>
{
    var indexPath = Path.Combine(app.Environment.WebRootPath, "index.html");
    if (File.Exists(indexPath))
    {
        context.Response.ContentType = "text/html";
        await context.Response.SendFileAsync(indexPath);
    }
    else
    {
        context.Response.StatusCode = 404;
    }
});

app.Run();
