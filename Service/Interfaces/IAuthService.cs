using Services.Commons;
using Services.DTOs.Requests;
using Services.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<LoginResponse>> LoginAsync(
          LoginRequest request);

        Task<ApiResponse<LoginResponse>> RefreshTokenAsync(
            string refreshToken);

        Task<ApiResponse<bool>> LogoutAsync(
            string refreshToken);

        Task<ApiResponse<bool>> RegisterAsync(
            RegisterRequest request);

        Task<ApiResponse<TokenStatusResponse>> GetTokenStatusAsync(
            System.Security.Claims.ClaimsPrincipal user);
    }
}
