using Services.Commons;
using Services.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<PagedResult<UserResponse>>> GetAllUsersAsync(int pageNumber = 1, int pageSize = 10);
        Task<ApiResponse<UserResponse>> GetUserByIdAsync(Guid id);
        Task<ApiResponse<bool>> ToggleUserLockAsync(Guid id);
    }
}
