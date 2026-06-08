using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace LoginFeature_TuanPM.Controllers
{
    /// <summary>
    /// Provides administrative user-management operations, including listing,
    /// retrieving, and toggling the lock state of user accounts.
    /// All endpoints are restricted to users with the <c>Admin</c> role.
    /// </summary>
    [ApiController]
    [Route("api/users")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsersController"/> class.
        /// </summary>
        /// <param name="userService">
        /// Service responsible for user-management business logic.
        /// </param>
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Returns a paginated list of all registered users.
        /// </summary>
        /// <param name="pageNumber">
        /// The 1-based index of the page to retrieve. Defaults to <c>1</c>.
        /// </param>
        /// <param name="pageSize">
        /// The maximum number of users to include per page. Defaults to <c>10</c>.
        /// </param>
        /// <returns>
        /// <c>200 OK</c> with a <c>PagedResult</c> containing users and total count;
        /// <c>400 Bad Request</c> if the query fails;
        /// <c>401 Unauthorized</c> if the caller is not authenticated;
        /// <c>403 Forbidden</c> if the caller does not have the <c>Admin</c> role.
        /// </returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllUsersAsync(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var response = await _userService.GetAllUsersAsync(pageNumber, pageSize);
            if (response.Error != null)
                return BadRequest(response);

            return Ok(response);
        }

        /// <summary>
        /// Retrieves detailed information about a specific user by their unique identifier.
        /// </summary>
        /// <param name="id">
        /// The <see cref="Guid"/> identifier of the target user.
        /// </param>
        /// <returns>
        /// <c>200 OK</c> with the user details object;
        /// <c>400 Bad Request</c> if the user is not found;
        /// <c>401 Unauthorized</c> if the caller is not authenticated;
        /// <c>403 Forbidden</c> if the caller does not have the <c>Admin</c> role.
        /// </returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetUserByIdAsync(Guid id)
        {
            var response = await _userService.GetUserByIdAsync(id);
            if (response.Error != null)
                return BadRequest(response);

            return Ok(response);
        }

        /// <summary>
        /// Toggles the lock state of a user account (locks an active account or unlocks a locked one).
        /// </summary>
        /// <remarks>
        /// When a user is locked, any active sessions are force-terminated via SignalR.
        /// This action is idempotent: calling it twice returns the account to its original state.
        /// </remarks>
        /// <param name="id">
        /// The <see cref="Guid"/> identifier of the user whose lock state will be toggled.
        /// </param>
        /// <returns>
        /// <c>200 OK</c> with the updated lock state;
        /// <c>400 Bad Request</c> if the user is not found or the operation fails;
        /// <c>401 Unauthorized</c> if the caller is not authenticated;
        /// <c>403 Forbidden</c> if the caller does not have the <c>Admin</c> role.
        /// </returns>
        [HttpPut("{id}/lock")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ToggleUserLockAsync(Guid id)
        {
            var response = await _userService.ToggleUserLockAsync(id);
            if (response.Error != null)
                return BadRequest(response);

            return Ok(response);
        }
    }
}
