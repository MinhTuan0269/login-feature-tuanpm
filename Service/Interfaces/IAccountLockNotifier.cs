using System;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IAccountLockNotifier
    {
        Task NotifyForceLogoutAsync(Guid userId);
    }
}
