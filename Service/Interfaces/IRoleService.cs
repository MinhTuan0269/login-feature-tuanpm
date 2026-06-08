using Services.Commons;
using Services.DTOs.Responses;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IRoleService
    {
        Task<ApiResponse<List<RoleResponse>>> GetAllRolesAsync();
    }
}
