using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Casan_IT15_Project.Hubs
{
    /// <summary>
    /// SignalR hub for real-time production updates.
    /// Broadcasts work order status changes and production schedule updates.
    /// </summary>
    [Authorize]
    public class ProductionHub : Hub
    {
        public async Task JoinCompanyGroup(string companyId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"company-{companyId}");
        }

        public async Task LeaveCompanyGroup(string companyId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"company-{companyId}");
        }

        public async Task SendWorkOrderUpdate(string companyId, object workOrderData)
        {
            await Clients.Group($"company-{companyId}").SendAsync("ReceiveWorkOrderUpdate", workOrderData);
        }

        public async Task SendProductionUpdate(string companyId, object productionData)
        {
            await Clients.Group($"company-{companyId}").SendAsync("ReceiveProductionUpdate", productionData);
        }

        public async Task SendDashboardRefresh(string companyId)
        {
            await Clients.Group($"company-{companyId}").SendAsync("RefreshDashboard");
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
