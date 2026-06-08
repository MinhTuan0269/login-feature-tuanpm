using Microsoft.EntityFrameworkCore;
using Repositories.DBContexts;
using Repositories.Interfaces;
using Repositories.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories
{
    /// <summary>
    /// Repository that handles data access operations for the Role entity.
    /// </summary>
    public class RoleRepo : IRoleRepo
    {
        private readonly TuanPmContext _context;

        public RoleRepo(TuanPmContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all roles from the database.
        /// </summary>
        /// <returns>A list of all Role entities in the system.</returns>
        public async Task<List<Role>> GetAllRolesAsync()
        {
            // Step 1: Query the entire Roles table and return the result as a list
            return await _context.Roles.ToListAsync();
        }
    }
}
