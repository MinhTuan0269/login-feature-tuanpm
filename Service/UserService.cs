using Repositories.Interfaces;
using Services.Commons;
using Services.DTOs.Responses;
using Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    /// <summary>
    /// Service that handles user management operations such as retrieval, pagination, and lock toggling.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepo _userRepo;
        private readonly IRefreshTokenRepo _refreshTokenRepo;
        private readonly IAccountLockNotifier _lockNotifier;

        public UserService(
            IUserRepo userRepo,
            IRefreshTokenRepo refreshTokenRepo,
            IAccountLockNotifier lockNotifier)
        {
            _userRepo = userRepo;
            _refreshTokenRepo = refreshTokenRepo;
            _lockNotifier = lockNotifier;
        }

        /// <summary>
        /// Retrieves a paginated list of all users, including locked accounts.
        /// </summary>
        /// <param name="pageNumber">The 1-based page number to retrieve (default: 1).</param>
        /// <param name="pageSize">The number of items per page (default: 10).</param>
        /// <returns>ApiResponse containing a PagedResult of UserResponse objects.</returns>
        public async Task<ApiResponse<PagedResult<UserResponse>>> GetAllUsersAsync(int pageNumber = 1, int pageSize = 10)
        {
            // Step 1: Fetch the paginated user list from the repository,
            //         including locked accounts and the total record count
            var (users, totalCount) = await _userRepo.GetAllUsersAsync(pageNumber, pageSize, includeLocked: true);

            // Step 2: Map each User entity to a UserResponse DTO,
            //         exposing only the fields needed by the client
            var response = users.Select(u => new UserResponse
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                RoleId = u.RoleId,
                RoleName = u.Role?.Name,
                IsLocked = u.IsLocked,
                LastLoginAt = u.LastLoginAt,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            }).ToList();

            // Step 3: Wrap the result set in a PagedResult object that carries pagination metadata
            var pagedResult = new PagedResult<UserResponse>
            {
                Items = response,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            // Step 4: Return a successful ApiResponse containing the paged data
            return new ApiResponse<PagedResult<UserResponse>>(
                message: "Users retrieved successfully",
                error: null,
                result: pagedResult
            );
        }

        /// <summary>
        /// Retrieves a single user by their unique identifier.
        /// </summary>
        /// <param name="id">The GUID of the user to retrieve.</param>
        /// <returns>ApiResponse containing the UserResponse, or an error if not found.</returns>
        public async Task<ApiResponse<UserResponse>> GetUserByIdAsync(Guid id)
        {
            // Step 1: Query the repository for the user by ID;
            //         includeLocked: true allows returning locked accounts as well
            var user = await _userRepo.GetUserByIdAsync(id, includeLocked: true);

            // Step 2: If the user does not exist, return a not-found error response
            if (user == null)
            {
                return new ApiResponse<UserResponse>(
                    message: "User retrieval failed",
                    error: "User not found",
                    result: null
                );
            }

            // Step 3: Map the User entity to a UserResponse DTO for the client
            var response = new UserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                RoleId = user.RoleId,
                RoleName = user.Role?.Name,
                IsLocked = user.IsLocked,
                LastLoginAt = user.LastLoginAt,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };

            // Step 4: Return a successful ApiResponse with the user data
            return new ApiResponse<UserResponse>(
                message: "User retrieved successfully",
                error: null,
                result: response
            );
        }

        /// <summary>
        /// Toggles the lock status of a user account.
        /// When the account becomes locked, all active refresh tokens are revoked
        /// and a force-logout notification is sent to connected clients via SignalR.
        /// </summary>
        /// <param name="id">The GUID of the user whose lock status should be toggled.</param>
        /// <returns>ApiResponse indicating success and a descriptive action message.</returns>
        public async Task<ApiResponse<bool>> ToggleUserLockAsync(Guid id)
        {
            // Step 1: Retrieve the user by ID, including locked accounts
            var user = await _userRepo.GetUserByIdAsync(id, includeLocked: true);

            // Step 2: If the user does not exist, return a not-found error response
            if (user == null)
            {
                return new ApiResponse<bool>(
                    message: "User lock toggle failed",
                    error: "User not found",
                    result: false
                );
            }

            // Step 3: Toggle the lock status and record the modification timestamp
            user.IsLocked = !user.IsLocked;
            user.UpdatedAt = DateTime.UtcNow;

            // Step 4: If the account was just locked, perform additional security actions
            if (user.IsLocked)
            {
                // Step 4a: Retrieve all refresh tokens belonging to this user
                var tokens = await _refreshTokenRepo.GetByUserIdAsync(user.Id);

                // Step 4b: Revoke every token that has not yet been revoked,
                //          forcing the user to be logged out from all devices
                foreach (var token in tokens)
                {
                    if (!token.IsRevoked)
                    {
                        token.IsRevoked = true;
                        token.RevokedAt = DateTime.UtcNow;
                        await _refreshTokenRepo.UpdateAsync(token);
                    }
                }

                // Step 4c: Send a real-time force-logout event to all connected clients via SignalR
                await _lockNotifier.NotifyForceLogoutAsync(user.Id);
            }

            // Step 5: Persist the updated lock status to the database
            await _userRepo.UpdateUserAsync(user);

            // Step 6: Return a success response with a message describing the action taken
            string action = user.IsLocked ? "locked" : "unlocked";
            return new ApiResponse<bool>(
                message: $"User successfully {action}",
                error: null,
                result: true
            );
        }
    }
}
