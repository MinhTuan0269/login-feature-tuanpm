using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.DTOs.Requests;
using Services.Interfaces;

namespace LoginFeature_TuanPM.Controllers
{
    /// <summary>
    /// Handles authentication-related operations including registration,
    /// login, token refresh, logout, and token validation.
    /// </summary>
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        /// <param name="authService">
        /// Service responsible for authentication and authorization operations.
        /// </param>
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Registers a new user account.
        /// </summary>
        /// <param name="request">
        /// Registration information including username, email, and password.
        /// </param>
        /// <returns>
        /// <c>200 OK</c> with the created user data on success;
        /// <c>400 Bad Request</c> if validation fails or the username/email is already taken.
        /// </returns>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterAsync(
            [FromBody] RegisterRequest request)
        {
            var response =
                await _authService.RegisterAsync(request);

            if (response.Error != null)
                return BadRequest(response);

            return Ok(response);
        }

        /// <summary>
        /// Authenticates a user and issues a JWT access token together with a refresh token.
        /// </summary>
        /// <param name="request">
        /// User login credentials (username/email and password).
        /// </param>
        /// <returns>
        /// <c>200 OK</c> with access and refresh tokens on success;
        /// <c>400 Bad Request</c> if credentials are invalid or the account is locked.
        /// </returns>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> LoginAsync(
            [FromBody] LoginRequest request)
        {
            var response =
                await _authService.LoginAsync(request);

            if (response.Error != null)
                return BadRequest(response);

            return Ok(response);
        }

        /// <summary>
        /// Generates a new access token and rotates the refresh token.
        /// </summary>
        /// <remarks>
        /// The old refresh token is invalidated after a successful call.
        /// Reusing a consumed refresh token will revoke the entire token family.
        /// </remarks>
        /// <param name="request">
        /// Object containing the current refresh token string.
        /// </param>
        /// <returns>
        /// <c>200 OK</c> with a new access token and refresh token pair;
        /// <c>400 Bad Request</c> if the refresh token is invalid, expired, or already consumed.
        /// </returns>
        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RefreshTokenAsync(
            [FromBody] RefreshTokenRequest request)
        {
            var response =
                await _authService.RefreshTokenAsync(
                    request.RefreshToken);

            if (response.Error != null)
                return BadRequest(response);

            return Ok(response);
        }

        /// <summary>
        /// Logs out the currently authenticated user and revokes the provided refresh token.
        /// </summary>
        /// <remarks>
        /// Requires a valid JWT access token in the <c>Authorization</c> header.
        /// Only the specific refresh token supplied in the request body is revoked;
        /// other active sessions remain unaffected.
        /// </remarks>
        /// <param name="request">
        /// Object containing the refresh token to be revoked.
        /// </param>
        /// <returns>
        /// <c>200 OK</c> on successful logout;
        /// <c>400 Bad Request</c> if the refresh token is not found or already revoked;
        /// <c>401 Unauthorized</c> if the access token is missing or invalid.
        /// </returns>
        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LogoutAsync(
            [FromBody] RefreshTokenRequest request)
        {
            var response =
                await _authService.LogoutAsync(
                    request.RefreshToken);

            if (response.Error != null)
                return BadRequest(response);

            return Ok(response);
        }

        /// <summary>
        /// Returns metadata about the currently authenticated access token.
        /// </summary>
        /// <remarks>
        /// Requires a valid JWT access token in the <c>Authorization</c> header.
        /// The response includes the remaining lifetime (in seconds) for both the
        /// access token and its associated refresh token.
        /// </remarks>
        /// <returns>
        /// <c>200 OK</c> with token details (expiry, remaining time, user claims);
        /// <c>400 Bad Request</c> if the token payload cannot be parsed;
        /// <c>401 Unauthorized</c> if no valid token is present.
        /// </returns>
        [Authorize]
        [HttpGet("token-status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetTokenStatusAsync()
        {
            var response =
                await _authService.GetTokenStatusAsync(User);

            if (response.Error == "Unauthorized")
                return Unauthorized();

            if (response.Error != null)
                return BadRequest(response);

            return Ok(response);
        }
    }
}
