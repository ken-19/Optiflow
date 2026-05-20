using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Casan_IT15_Project.Data;
using Casan_IT15_Project.DTOs.Auth;
using Casan_IT15_Project.Models;

namespace Casan_IT15_Project.Services
{
    /// <summary>
    /// Handles JWT authentication, login, and user registration.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<TokenResponseDto?> LoginAsync(LoginDto loginDto)
        {
            // Find user by username or email
            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => (u.Username == loginDto.Username || u.Email == loginDto.Username) && u.IsActive);

            if (user == null)
                return null;

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                return null;

            // Check subscription if user is an Admin
            var isAdmin = user.UserRoles.Any(ur => ur.RoleId == 2);
            if (isAdmin && user.Company != null)
            {
                if (user.Company.SubscriptionExpiry < DateTime.UtcNow)
                {
                    throw new Exception("Subscription has expired. Please renew your plan.");
                }
            }

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;
            
            // Log Action
            _context.SystemLogs.Add(new SystemLog
            {
                Action = "Login",
                Message = $"User {user.Username} logged in successfully.",
                UserId = user.UserId,
                CompanyId = user.CompanyId,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            // Generate JWT
            return GenerateTokenResponse(user);
        }

        public async Task<TokenResponseDto?> RegisterAsync(RegisterDto registerDto)
        {
            // Check if username or email already exists
            if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
                return null;
            if (await _context.Users.AnyAsync(u => u.Email == registerDto.Email))
                return null;

            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                CompanyId = registerDto.CompanyId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Assign role if specified (default to Plant Manager/Viewer)
            var roleId = registerDto.RoleId ?? 8;

            // Security: Prevent anyone from registering as Super Admin (RoleId 1)
            if (roleId == 1)
            {
                roleId = 8; // fallback to a safe role
            }
            _context.UserRoles.Add(new UserRole
            {
                UserId = user.UserId,
                RoleId = roleId,
                AssignedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            // Log Action
            _context.SystemLogs.Add(new SystemLog
            {
                Action = "User Registration",
                Message = $"User registered: {user.Username}",
                UserId = user.UserId,
                CompanyId = user.CompanyId,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            // Reload with navigation properties
            user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
                .Include(u => u.Company)
                .FirstAsync(u => u.UserId == user.UserId);

            return GenerateTokenResponse(user);
        }

        public async Task<TokenResponseDto?> RegisterTenantAsync(RegisterTenantDto dto)
        {
            // Check if username or email already exists
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
                return null;
            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return null;

            // Create Company first
            var company = new Company
            {
                CompanyName = dto.CompanyName,
                SubscriptionPlan = dto.SubscriptionPlan,
                SubscriptionExpiry = DateTime.UtcNow.AddYears(1), // Default 1 year subscription
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Companies.Add(company);
            await _context.SaveChangesAsync(); // To get the CompanyId

            // Create Admin User
            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                CompanyId = company.CompanyId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Assign Admin Role (RoleId 2 is Admin)
            _context.UserRoles.Add(new UserRole
            {
                UserId = user.UserId,
                RoleId = 2,
                AssignedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            // Log Action
            _context.SystemLogs.Add(new SystemLog
            {
                Action = "Tenant Registration",
                Message = $"Tenant registered: {company.CompanyName} by {user.Username}",
                UserId = user.UserId,
                CompanyId = company.CompanyId,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            // Reload with navigation properties
            user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                        .ThenInclude(r => r.RolePermissions)
                            .ThenInclude(rp => rp.Permission)
                .Include(u => u.Company)
                .FirstAsync(u => u.UserId == user.UserId);

            return GenerateTokenResponse(user);
        }

        private TokenResponseDto GenerateTokenResponse(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["Secret"] ?? "OptiFlowSuperSecretKey2026!@#$%^&*()DefaultKey";
            var issuer = jwtSettings["Issuer"] ?? "OptiFlow";
            var audience = jwtSettings["Audience"] ?? "OptiFlowClient";
            var expiryMinutes = int.Parse(jwtSettings["ExpiryMinutes"] ?? "480");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Build claims
            var roles = user.UserRoles.Select(ur => ur.Role.RoleName).ToList();
            var permissions = user.UserRoles
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission.PermissionName)
                .Distinct()
                .ToList();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("CompanyId", user.CompanyId?.ToString() ?? "")
            };

            // Add role claims
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            // Add permission claims
            foreach (var permission in permissions)
                claims.Add(new Claim("Permission", permission));

            var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials
            );

            return new TokenResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresAt = expiresAt,
                User = new UserInfoDto
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Roles = roles,
                    Permissions = permissions,
                    CompanyId = user.CompanyId,
                    CompanyName = user.Company?.CompanyName
                }
            };
        }
        public async Task<string> EmergencySeedAsync()
        {
            var reports = new List<string>();

            // 1. Ensure Roles exist
            var roles = new[]
            {
                new Role { RoleId = 1, RoleName = "Super Admin", Description = "Full system access across all companies", CreatedAt = DateTime.UtcNow },
                new Role { RoleId = 2, RoleName = "Admin", Description = "Company owner with full company access", CreatedAt = DateTime.UtcNow }
            };

            foreach (var role in roles)
            {
                if (!await _context.Roles.AnyAsync(r => r.RoleId == role.RoleId))
                {
                    _context.Roles.Add(role);
                    reports.Add($"Added Role: {role.RoleName}");
                }
            }
            await _context.SaveChangesAsync();

            // 2. Ensure Companies exist
            if (!await _context.Companies.AnyAsync(c => c.CompanyId == 2))
            {
                _context.Companies.Add(new Company
                {
                    CompanyId = 2,
                    CompanyName = "Jetro Manufacturing",
                    Address = "456 Production Blvd, Cebu, Philippines",
                    ContactEmail = "admin_jetro@gmail.com",
                    IsActive = true,
                    SubscriptionPlan = "Premium Plan",
                    SubscriptionExpiry = DateTime.UtcNow.AddYears(1),
                    CreatedAt = DateTime.UtcNow
                });
                reports.Add("Added Company: Jetro Manufacturing");
                await _context.SaveChangesAsync();
            }

            // 3. Ensure Users exist
            var seedUsers = new[]
            {
                new User
                {
                    UserId = 1,
                    Username = "superadmin",
                    Email = "superadmin@optiflow.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    FirstName = "Super",
                    LastName = "Admin",
                    IsActive = true,
                    CompanyId = null,
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    UserId = 2,
                    Username = "admin_jetro@gmail.com",
                    Email = "admin_jetro@gmail.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                    FirstName = "Admin",
                    LastName = "Jetro",
                    IsActive = true,
                    CompanyId = 2,
                    CreatedAt = DateTime.UtcNow
                }
            };

            foreach (var user in seedUsers)
            {
                if (!await _context.Users.AnyAsync(u => u.Username == user.Username))
                {
                    _context.Users.Add(user);
                    reports.Add($"Added User: {user.Username}");
                    await _context.SaveChangesAsync();

                    // Assign Role
                    var roleId = user.UserId == 1 ? 1 : 2;
                    _context.UserRoles.Add(new UserRole
                    {
                        UserId = user.UserId,
                        RoleId = roleId,
                        AssignedAt = DateTime.UtcNow
                    });
                    reports.Add($"Assigned Role {roleId} to {user.Username}");
                }
            }
            await _context.SaveChangesAsync();

            return reports.Count > 0 ? string.Join(", ", reports) : "Database already fully seeded.";
        }
    }
}
