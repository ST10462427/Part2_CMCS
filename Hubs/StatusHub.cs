using Microsoft.AspNetCore.SignalR;


namespace Part2_CMCS.Hubs
{
    public class StatusHub : Hub
    {
        public async Task NotifyStatusChanged(int claimId, string newStatus)
        {
            await Clients.All.SendAsync("ClaimStatusChanged", claimId, newStatus);
        }
    }
}