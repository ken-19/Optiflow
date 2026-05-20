using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Casan_IT15_Project.Data;
using Casan_IT15_Project.DTOs;
using Casan_IT15_Project.Models;
using Casan_IT15_Project.Services;

namespace Casan_IT15_Project.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ISystemLogService _logService;

        public UsersController(ApplicationDbContext context, ISystemLogService logService)
        {
            _context = context;
            _logService = logService;
        }

        [HttpGet]
        [Authorize(Roles = "Super Admin,Admin")]
        public async Task<ActionResult<List<UserDto>>> GetUsers()
        {
            var companyId = GetCompanyId();
            var query = _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(u => u.Company)
                .AsQueryable();

            if (!User.IsInRole("Super Admin"))
            {
                query = query.Where(u => u.CompanyId == companyId);
                // Company admins must never see Super Admin accounts
                query = query.Where(u => !u.UserRoles.Any(ur => ur.Role.RoleName == "Super Admin"));
            }

            var users = await query.Select(u => new UserDto
            {
                UserId = u.UserId,
                Username = u.Username,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Phone = u.Phone,
                IsActive = u.IsActive,
                CompanyId = u.CompanyId,
                CompanyName = u.Company != null ? u.Company.CompanyName : null,
                Roles = u.UserRoles.Select(ur => ur.Role.RoleName).ToList(),
                CreatedAt = u.CreatedAt
            }).ToListAsync();

            return Ok(users);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Super Admin,Admin")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null) return NotFound();

            // Company isolation: non-Super Admin can only view users in their own company
            if (!User.IsInRole("Super Admin"))
            {
                if (user.CompanyId != GetCompanyId())
                    return Forbid();
                // Prevent company admins from viewing Super Admin accounts
                if (user.UserRoles.Any(ur => ur.Role.RoleName == "Super Admin"))
                    return Forbid();
            }

            return Ok(new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Phone = user.Phone,
                IsActive = user.IsActive,
                CompanyId = user.CompanyId,
                CompanyName = user.Company?.CompanyName,
                Roles = user.UserRoles.Select(ur => ur.Role.RoleName).ToList(),
                CreatedAt = user.CreatedAt
            });
        }

        [HttpGet("roles")]
        [Authorize(Roles = "Super Admin,Admin")]
        public async Task<IActionResult> GetRoles()
        {
            var query = _context.Roles.AsQueryable();

            if (!User.IsInRole("Super Admin"))
            {
                // Admin users cannot see or assign the Super Admin role (RoleId = 1)
                query = query.Where(r => r.RoleId != 1);
            }

            var roles = await query.Select(r => new { r.RoleId, r.RoleName }).ToListAsync();
            return Ok(roles);
        }

        [HttpPost]
        [Authorize(Roles = "Super Admin,Admin")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username || u.Email == dto.Email))
            {
                return Conflict("Username or email already exists");
            }

            var companyId = GetCompanyId();
            var targetCompanyId = dto.CompanyId;

            if (!User.IsInRole("Super Admin"))
            {
                // Admins can only create users under their own company
                targetCompanyId = companyId;
            }
            else if (!targetCompanyId.HasValue)
            {
                // Super Admin MUST provide a CompanyId to assign the user to
                return BadRequest("CompanyId is required when created by Super Admin");
            }

            // Role Security
            if (!User.IsInRole("Super Admin") && dto.RoleId == 1)
            {
                return Forbid("Only Super Admin can assign the Super Admin role.");
            }

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                CompanyId = targetCompanyId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            if (dto.RoleId.HasValue)
            {
                _context.UserRoles.Add(new UserRole { UserId = user.UserId, RoleId = dto.RoleId.Value });
                await _context.SaveChangesAsync();
            }

            await _logService.LogAsync("Create User", $"Created user {user.Username} (ID: {user.UserId})", GetUserId(), targetCompanyId);

            return CreatedAtAction(nameof(GetUser), new { id = user.UserId }, new { user.UserId, user.Username });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Super Admin,Admin")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            // Company isolation: non-Super Admin can only edit users in their own company
            if (!User.IsInRole("Super Admin") && user.CompanyId != GetCompanyId())
                return Forbid();

            if (dto.Email != null) user.Email = dto.Email;
            if (dto.FirstName != null) user.FirstName = dto.FirstName;
            if (dto.LastName != null) user.LastName = dto.LastName;
            if (dto.Phone != null) user.Phone = dto.Phone;
            if (dto.IsActive.HasValue) user.IsActive = dto.IsActive.Value;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            await _logService.LogAsync("Update User", $"Updated user {user.Username} (ID: {id})", GetUserId(), user.CompanyId);

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Super Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            
            await _logService.LogAsync("Delete User", $"Deleted user {user.Username} (ID: {id})", GetUserId(), user.CompanyId);

            return NoContent();
        }

        [HttpPost("{id}/roles/{roleId}")]
        [Authorize(Roles = "Super Admin,Admin")]
        public async Task<IActionResult> AssignRole(int id, int roleId)
        {
            if (!User.IsInRole("Super Admin") && roleId == 1)
            {
                return Forbid("Only Super Admin can assign the Super Admin role.");
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return NotFound("User not found");

            // Company isolation: non-Super Admin can only assign roles to users in their company
            if (!User.IsInRole("Super Admin") && user.CompanyId != GetCompanyId())
                return Forbid();

            if (!await _context.Roles.AnyAsync(r => r.RoleId == roleId))
                return NotFound("Role not found");

            if (await _context.UserRoles.AnyAsync(ur => ur.UserId == id && ur.RoleId == roleId))
                return Conflict("Role already assigned");

            _context.UserRoles.Add(new UserRole { UserId = id, RoleId = roleId });
            await _context.SaveChangesAsync();
            
            await _logService.LogAsync("Assign Role", $"Assigned Role {roleId} to User {id}", GetUserId(), user.CompanyId);
            
            return Ok();
        }

        private int? GetCompanyId()
        {
            var claim = User.FindFirst("CompanyId")?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }

        private int? GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }
    }
}
