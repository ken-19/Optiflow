using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Casan_IT15_Project.Hubs
{
    /// <summary>
    /// SignalR hub for real-time notifications.
    /// Sends alerts for quality issues, low stock, etc.
    /// </summary>
    [Authorize]
    public class NotificationHub : Hub
    {
        public async Task SendNotification(string companyId, string message, string type)
        {
            await Clients.Group($"company-{companyId}").SendAsync("ReceiveNotification", new
            {
                message,
                type,
                timestamp = DateTime.UtcNow
            });
        }

        public async Task SendQualityAlert(string companyId, object alertData)
        {
            await Clients.Group($"company-{companyId}").SendAsync("ReceiveQualityAlert", alertData);
        }

        public async Task SendLowStockAlert(string companyId, object stockData)
        {
            await Clients.Group($"company-{companyId}").SendAsync("ReceiveLowStockAlert", stockData);
        }

        public override async Task OnConnectedAsync()
        {
            var companyId = Context.User?.FindFirst("CompanyId")?.Value;
            if (!string.IsNullOrEmpty(companyId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"company-{companyId}");
            }
            await base.OnConnectedAsync();
        }
    }
}
