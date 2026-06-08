using Repositories.Interfaces;
using Repositories.Models;
using Services.Commons;
using Services.DTOs.Requests;
using Services.DTOs.Responses;
using Services.Helper;
using Services.Interfaces;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Services
{
    /// <summary>
    /// Service that handles authentication operations such as login, logout,
    /// token refresh, registration, and token status retrieval.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IJwtService _jwtService;
        private readonly IUserRepo _userRepo;
        private readonly IRefreshTokenRepo _refreshTokenRepo;
        private readonly IAccountLockNotifier _lockNotifier;

        public AuthService(
            IJwtService jwtService,
            IUserRepo userRepo,
            IRefreshTokenRepo refreshTokenRepo,
            IAccountLockNotifier lockNotifier)
        {
            _jwtService = jwtService;
            _userRepo = userRepo;
            _refreshTokenRepo = refreshTokenRepo;
            _lockNotifier = lockNotifier;
        }

        /// <summary>
        /// Authenticates a user with email and password.
        /// </summary>
        /// <param name="request">Login request containing email and password.</param>
        /// <returns>ApiResponse containing login details or error information.</returns>
        public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
        {
            // Step 1: Retrieve the user by email from the repository
            var user = await _userRepo.GetUserByEmailAsync(request.Email);

            // Step 2: If user not found, return a generic invalid-credentials error
            if (user == null)
            {
                return new ApiResponse<LoginResponse>(
                    message: "Login failed",
                    error: "Invalid email or password",
                    result: null
                );
            }

            // Step 3: Check if the account is locked before proceeding
            if (user.IsLocked)
            {
                return new ApiResponse<LoginResponse>(
                    message: "Login failed",
                    error: "Account is locked"
                );
            }

            // Step 4: Verify the supplied password against the stored BCrypt hash
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            // Step 5: Handle invalid password — increment failure counter and possibly lock the account
            if (!isPasswordValid)
            {
                // Increment the failed-login attempt counter
                user.FailedLoginAttempts++;

                // Step 5a: If the threshold (5 attempts) is reached, lock the account
                //          and revoke all active refresh tokens
                if (user.FailedLoginAttempts >= 5)
                {
                    user.IsLocked = true;

                    var tokens = await _refreshTokenRepo.GetByUserIdAsync(user.Id);

                    foreach (var token in tokens)
                    {
                        token.IsRevoked = true;
                        token.RevokedAt = DateTime.UtcNow;
                        await _refreshTokenRepo.UpdateAsync(token);
                    }

                    // Step 5b: Notify connected clients to force-logout via SignalR
                    await _lockNotifier.NotifyForceLogoutAsync(user.Id);
                }

                // Persist the updated failed-attempt count and lock status
                await _userRepo.UpdateUserAsync(user);

                // Return an informative error showing the number of remaining attempts
                return new ApiResponse<LoginResponse>(
                    message: "Login failed",
                    error: $"Invalid password. Remaining attempts: {5 - user.FailedLoginAttempts}"
                );
            }

            // Step 6: Authentication succeeded — reset the failure counter and record the login time
            user.FailedLoginAttempts = 0;
            user.LastLoginAt = DateTime.UtcNow;

            // Persist the updated user state to the database
            await _userRepo.UpdateUserAsync(user);

            // Step 7: Generate a short-lived JWT access token for the authenticated user
            string accessToken = _jwtService.GenerateToken(user);

            // Step 8: Create a new refresh token record valid for 7 days
            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = Guid.NewGuid().ToString(),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsRevoked = false
            };

            // Persist the new refresh token to the database
            await _refreshTokenRepo.CreateAsync(refreshToken);

            // Step 9: Assemble the response DTO with user information and both tokens
            var response = new LoginResponse
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.Name,
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                FailedLoginAttempts = user.FailedLoginAttempts
            };

            // Step 10: Return a successful login response
            return new ApiResponse<LoginResponse>(
                message: "Login successfully",
                error: null,
                result: response
            );
        }

        /// <summary>
        /// Logs out a user by revoking the provided refresh token.
        /// </summary>
        /// <param name="refreshToken">The refresh token string to revoke.</param>
        /// <returns>ApiResponse indicating success or failure.</returns>
        public async Task<ApiResponse<bool>> LogoutAsync(string refreshToken)
        {
            // Step 1: Look up the refresh token entity in the database by its value
            var token = await _refreshTokenRepo.GetByTokenAsync(refreshToken);

            // Step 2: If the token does not exist, return an invalid-token error
            if (token == null)
            {
                return new ApiResponse<bool>(
                    message: "Logout failed",
                    error: "Invalid refresh token",
                    result: false
                );
            }

            // Step 3: Mark the token as revoked and record the revocation timestamp
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;

            // Step 4: Persist the revoked status to the database
            await _refreshTokenRepo.UpdateAsync(token);

            // Step 5: Return a successful logout response
            return new ApiResponse<bool>(
                message: "Logout successfully",
                error: null,
                result: true
            );
        }

        /// <summary>
        /// Issues a new access token using a valid refresh token.
        /// </summary>
        /// <param name="refreshToken">The refresh token string provided by the client.</param>
        /// <returns>ApiResponse containing refreshed login information or error details.</returns>
        public async Task<ApiResponse<LoginResponse>> RefreshTokenAsync(string refreshToken)
        {
            // Step 1: Retrieve the refresh token entity from the repository
            var token = await _refreshTokenRepo.GetByTokenAsync(refreshToken);

            // Step 2: Validate that the token exists
            if (token == null)
            {
                return new ApiResponse<LoginResponse>(
                    message: "Refresh token failed",
                    error: "Invalid refresh token",
                    result: null
                );
            }

            // Step 3: Ensure the token is neither revoked nor expired
            if (token.IsRevoked || token.ExpiresAt < DateTime.UtcNow)
            {
                return new ApiResponse<LoginResponse>(
                    message: "Refresh token failed",
                    error: "Refresh token is revoked or expired",
                    result: null
                );
            }

            // Step 4: Retrieve the user associated with this token
            var user = await _userRepo.GetUserByIdAsync(token.UserId);
            if (user == null)
            {
                return new ApiResponse<LoginResponse>(
                    message: "Refresh token failed",
                    error: "User not found",
                    result: null
                );
            }

            // Step 5: Verify the user account is not locked
            if (user.IsLocked)
            {
                return new ApiResponse<LoginResponse>(
                    message: "Refresh token failed",
                    error: "Account is locked",
                    result: null
                );
            }

            // Step 6: Generate a new short-lived access token for the user
            string newAccessToken = _jwtService.GenerateToken(user);

            // Step 7: Build the response DTO containing user info and both tokens
            var response = new LoginResponse
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.Name,
                AccessToken = newAccessToken,
                RefreshToken = token.Token,
                FailedLoginAttempts = user.FailedLoginAttempts
            };

            // Step 8: Return a successful API response
            return new ApiResponse<LoginResponse>(
                message: "Refresh token successfully",
                error: null,
                result: response
            );
        }

        /// <summary>
        /// Registers a new user account.
        /// </summary>
        /// <param name="request">Registration request containing user details.</param>
        /// <returns>ApiResponse indicating whether registration succeeded.</returns>
        public async Task<ApiResponse<bool>> RegisterAsync(RegisterRequest request)
        {
            // Step 1: Check whether the email address is already registered
            var existingUser = await _userRepo.GetUserByEmailAsync(request.Email);

            // Step 2: If the email already exists, return a duplicate-email error
            if (existingUser != null)
            {
                return new ApiResponse<bool>(
                    message: "Register failed",
                    error: "Email already exists",
                    result: false
                );
            }

            // Step 3: Validate the password against the security policy
            //         (minimum 12 characters, at least 1 uppercase letter and 1 special character)
            if (!PasswordValidator.IsValid(request.Password))
            {
                return new ApiResponse<bool>(
                    message: "Register failed",
                    error: "Password must contain at least 12 characters, 1 uppercase letter and 1 special character",
                    result: false
                );
            }

            // Step 4: Build a new User entity with the password hashed via BCrypt
            //         IsLocked and FailedLoginAttempts are set to their safe defaults
            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                RoleId = request.RoleId,
                IsLocked = false,
                FailedLoginAttempts = 0
            };

            // Step 5: Persist the new user to the database via the repository
            await _userRepo.CreateUserAsync(user);

            // Step 6: Return a successful registration response
            return new ApiResponse<bool>(
                message: "Register successfully",
                error: null,
                result: true
            );
        }

        /// <summary>
        /// Retrieves the remaining validity period of the current access and refresh tokens.
        /// </summary>
        /// <param name="userPrincipal">ClaimsPrincipal representing the authenticated user.</param>
        /// <returns>ApiResponse containing token expiration details.</returns>
        public async Task<ApiResponse<TokenStatusResponse>> GetTokenStatusAsync(ClaimsPrincipal userPrincipal)
        {
            // Step 1: Extract the user identifier claim from the principal
            var userIdStr = userPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            // Step 2: Validate that the claim exists and can be parsed to a GUID;
            //         return Unauthorized if not
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                return new ApiResponse<TokenStatusResponse>(
                    message: "Failed to get token status",
                    error: "Unauthorized",
                    result: null
                );
            }

            // Step 3: Read the "exp" claim to determine the access token's expiry (Unix epoch seconds)
            long accessTokenExp = 0;
            var expClaim = userPrincipal.FindFirst("exp")?.Value;
            if (expClaim != null)
            {
                long.TryParse(expClaim, out accessTokenExp);
            }

            // Step 4: Fetch all refresh tokens that belong to this user
            var tokens = await _refreshTokenRepo.GetByUserIdAsync(userId);

            // Step 5: Select the most recently expiring, non-revoked, still-valid refresh token
            var activeToken = tokens
                .Where(t => !t.IsRevoked && t.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(t => t.ExpiresAt)
                .FirstOrDefault();

            // Step 6: If no active refresh token exists, return an appropriate error response
            if (activeToken == null)
            {
                return new ApiResponse<TokenStatusResponse>(
                    message: "Failed to get token status",
                    error: "No active refresh token found for the user",
                    result: null
                );
            }

            // Step 7: Convert the access token expiry from Unix epoch seconds to a DateTimeOffset
            //         and calculate the remaining seconds for both tokens
            DateTimeOffset accessExpDate = DateTimeOffset.FromUnixTimeSeconds(accessTokenExp);
            double accessRemain = (accessExpDate - DateTimeOffset.UtcNow).TotalSeconds;
            double refreshRemain = (activeToken.ExpiresAt - DateTime.UtcNow).TotalSeconds;

            // Step 8: Build the response DTO with the calculated expiry information
            var response = new TokenStatusResponse
            {
                AccessTokenExpiresAt = accessExpDate.UtcDateTime,
                AccessTokenRemainingSeconds = accessRemain > 0 ? accessRemain : 0,
                RefreshTokenExpiresAt = activeToken.ExpiresAt,
                RefreshTokenRemainingSeconds = refreshRemain > 0 ? refreshRemain : 0
            };

            // Step 9: Return a successful API response containing the token status
            return new ApiResponse<TokenStatusResponse>(
                message: "Token status retrieved successfully",
                error: null,
                result: response
            );
        }
    }
}