using Casan_IT15_Project.Models;

namespace Casan_IT15_Project.Services
{
    public interface ISystemLogService
    {
        Task LogAsync(string action, string message, int? userId = null, int? companyId = null, string logLevel = "Info", string? source = null);
    }
}
