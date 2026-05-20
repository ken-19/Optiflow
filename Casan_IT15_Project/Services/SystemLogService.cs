using Casan_IT15_Project.Data;
using Casan_IT15_Project.Models;

namespace Casan_IT15_Project.Services
{
    public class SystemLogService : ISystemLogService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public SystemLogService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task LogAsync(string action, string message, int? userId = null, int? companyId = null, string logLevel = "Info", string? source = null)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            var log = new SystemLog
            {
                Action = action,
                Message = message,
                UserId = userId,
                CompanyId = companyId,
                LogLevel = logLevel,
                Source = source,
                CreatedAt = DateTime.UtcNow
            };
            
            context.SystemLogs.Add(log);
            await context.SaveChangesAsync();
        }
    }
}
