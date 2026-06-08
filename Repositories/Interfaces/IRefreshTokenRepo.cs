using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IRefreshTokenRepo
    {
        Task CreateAsync(RefreshToken token);

        Task<RefreshToken?> GetByTokenAsync(string token);

        Task<List<RefreshToken>> GetByUserIdAsync(Guid userId);

        Task UpdateAsync(RefreshToken token);
    }
}
