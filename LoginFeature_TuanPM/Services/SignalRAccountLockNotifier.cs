using LoginFeature_TuanPM.Hubs;
using Microsoft.AspNetCore.SignalR;
using Services.Interfaces;

namespace LoginFeature_TuanPM.Services
{
    public class SignalRAccountLockNotifier : IAccountLockNotifier
    {
        private readonly IHubContext<AuthHub> _hubContext;

        public SignalRAccountLockNotifier(IHubContext<AuthHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyForceLogoutAsync(Guid userId)
        {
            await _hubContext.Clients
                .Group(userId.ToString())
                .SendAsync("ForceLogout");
        }
    }
}
