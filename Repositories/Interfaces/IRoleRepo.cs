using System.Collections.Generic;
using System.Threading.Tasks;
using Repositories.Models;

namespace Repositories.Interfaces
{
    public interface IRoleRepo
    {
        Task<List<Role>> GetAllRolesAsync();
    }
}
