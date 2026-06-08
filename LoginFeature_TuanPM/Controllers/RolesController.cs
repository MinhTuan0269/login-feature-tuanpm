using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using System.Threading.Tasks;

namespace LoginFeature_TuanPM.Controllers
{
    /// <summary>
    /// Manages role-related operations, such as retrieving all available roles.
    /// All endpoints require an authenticated user (JWT bearer token).
    /// </summary>
    [ApiController]
    [Route("api/roles")]
    [Authorize]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;

        /// <summary>
        /// Initializes a new instance of the <see cref="RolesController"/> class.
        /// </summary>
        /// <param name="roleService">
        /// Service responsible for role management operations.
        /// </param>
        public RolesController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        /// <summary>
        /// Retrieves the full list of roles defined in the system.
        /// </summary>
        /// <remarks>
        /// Requires a valid JWT access token in the <c>Authorization</c> header.
        /// Typically used to populate role selection drop-downs in the admin UI.
        /// </remarks>
        /// <returns>
        /// <c>200 OK</c> with a list of role objects;
        /// <c>400 Bad Request</c> if the query cannot be completed;
        /// <c>401 Unauthorized</c> if the caller is not authenticated.
        /// </returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAllRolesAsync()
        {
            var response = await _roleService.GetAllRolesAsync();
            if (response.Error != null)
                return BadRequest(response);

            return Ok(response);
        }
    }
}
