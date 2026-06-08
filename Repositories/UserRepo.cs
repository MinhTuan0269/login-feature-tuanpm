using Microsoft.EntityFrameworkCore;
using Repositories.DBContexts;
using Repositories.Interfaces;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories
{
    /// <summary>
    /// Repository that handles data access operations for the User entity.
    /// </summary>
    public class UserRepo : IUserRepo
    {
        private readonly TuanPmContext _context;

        public UserRepo(TuanPmContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates a new user and persists it to the database.
        /// </summary>
        /// <param name="user">The User entity to create.</param>
        public async Task CreateUserAsync(User user)
        {
            // Step 1: Add the new User entity to the DbSet (staged, not yet written to the DB)
            await _context.Users.AddAsync(user);

            // Step 2: Commit the staged changes to the database (executes INSERT)
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves a paginated list of users, optionally including locked accounts.
        /// </summary>
        /// <param name="pageNumber">The 1-based page index.</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <param name="includeLocked">When true, locked accounts are included in the result.</param>
        /// <returns>A tuple containing the paged user list and the total record count.</returns>
        public async Task<(List<User> Items, int TotalCount)> GetAllUsersAsync(
            int pageNumber,
            int pageSize,
            bool includeLocked = false)
        {
            // Step 1: Start the query with eager-loading of the related Role entity
            var query = _context.Users.Include(u => u.Role).AsQueryable();

            // Step 2: If locked accounts should be excluded, filter them out
            if (!includeLocked)
            {
                query = query.Where(u => !u.IsLocked);
            }

            // Step 3: Count the total number of matching records for pagination metadata
            int totalCount = await query.CountAsync();

            // Step 4: Apply ordering and pagination (Skip/Take), then materialise the query
            var items = await query
                .OrderBy(u => u.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Step 5: Return the paged items together with the total record count
            return (items, totalCount);
        }

        /// <summary>
        /// Retrieves a user by their email address, including the related Role.
        /// </summary>
        /// <param name="email">The email address to search for.</param>
        /// <returns>The matching User entity, or null if not found.</returns>
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            // Step 1: Query for the user whose email matches, eagerly loading the Role,
            //         and return the first match or null
            return await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Email == email)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Retrieves a user by their unique identifier, optionally including locked accounts.
        /// </summary>
        /// <param name="id">The GUID that uniquely identifies the user.</param>
        /// <param name="includeLocked">When true, a locked account may also be returned.</param>
        /// <returns>The matching User entity, or null if not found.</returns>
        public async Task<User?> GetUserByIdAsync(Guid id, bool includeLocked = false)
        {
            // Step 1: Build the base query filtered by ID, with the related Role eager-loaded
            var query = _context.Users
                .Include(u => u.Role)
                .Where(u => u.Id == id);

            // Step 2: If locked accounts should be excluded, add an IsLocked filter
            if (!includeLocked)
            {
                query = query.Where(u => !u.IsLocked);
            }

            // Step 3: Execute the query and return the first matching record (or null)
            return await query.FirstOrDefaultAsync();
        }

        /// <summary>
        /// Updates an existing user record in the database.
        /// </summary>
        /// <param name="user">The User entity with updated values.</param>
        public Task UpdateUserAsync(User user)
        {
            // Step 1: Mark the User entity as modified in the DbContext change tracker
            _context.Users.Update(user);

            // Step 2: Commit the change to the database (executes UPDATE)
            return _context.SaveChangesAsync();
        }
    }
}
