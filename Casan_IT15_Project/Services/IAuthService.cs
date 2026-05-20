using Casan_IT15_Project.DTOs.Auth;

namespace Casan_IT15_Project.Services
{
    public interface IAuthService
    {
        Task<TokenResponseDto?> LoginAsync(LoginDto loginDto);
        Task<TokenResponseDto?> RegisterAsync(RegisterDto registerDto);
        Task<TokenResponseDto?> RegisterTenantAsync(RegisterTenantDto registerTenantDto);
        Task<string> EmergencySeedAsync();
    }
}
