using Repositories.Interfaces;
using Services.Commons;
using Services.DTOs.Responses;
using Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    /// <summary>
    /// Service that handles operations related to user roles.
    /// </summary>
    public class RoleService : IRoleService
    {
        private readonly IRoleRepo _roleRepo;

        public RoleService(IRoleRepo roleRepo)
        {
            _roleRepo = roleRepo;
        }

        /// <summary>
        /// Retrieves all roles available in the system.
        /// </summary>
        /// <returns>ApiResponse containing a list of RoleResponse objects.</returns>
        public async Task<ApiResponse<List<RoleResponse>>> GetAllRolesAsync()
        {
            // Step 1: Query all Role records from the database via the repository
            var roles = await _roleRepo.GetAllRolesAsync();

            // Step 2: Map each Role entity to a RoleResponse DTO,
            //         exposing only the fields required by the client
            var response = roles.Select(r => new RoleResponse
            {
                Id = r.Id,
                Name = r.Name
            }).ToList();

            // Step 3: Return a successful ApiResponse containing the role list
            return new ApiResponse<List<RoleResponse>>(
                message: "Roles retrieved successfully",
                error: null,
                result: response
            );
        }
    }
}
