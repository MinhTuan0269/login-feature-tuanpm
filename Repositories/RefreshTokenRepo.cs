using Microsoft.EntityFrameworkCore;
using Repositories.DBContexts;
using Repositories.Interfaces;
using Repositories.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories
{
    /// <summary>
    /// Repository that handles CRUD operations for the RefreshToken entity.
    /// </summary>
    public class RefreshTokenRepo : IRefreshTokenRepo
    {
        private readonly TuanPmContext _context;

        public RefreshTokenRepo(TuanPmContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Creates a new refresh token and persists it to the database.
        /// </summary>
        /// <param name="token">The RefreshToken entity to create.</param>
        public async Task CreateAsync(RefreshToken token)
        {
            // Step 1: Add the new RefreshToken entity to the DbSet (staged, not yet written to the DB)
            await _context.RefreshTokens.AddAsync(token);

            // Step 2: Commit the staged changes to the database (executes INSERT)
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Retrieves a refresh token by its string value.
        /// </summary>
        /// <param name="token">The token string to search for.</param>
        /// <returns>The matching RefreshToken entity, or null if not found.</returns>
        public Task<RefreshToken?> GetByTokenAsync(string token)
        {
            // Step 1: Query for the first RefreshToken whose Token value matches the supplied string;
            //         returns null if no match is found
            return _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == token);
        }

        /// <summary>
        /// Retrieves all refresh tokens associated with a specific user.
        /// </summary>
        /// <param name="userId">The GUID of the user whose tokens should be retrieved.</param>
        /// <returns>A list of all RefreshToken entities belonging to the user.</returns>
        public Task<List<RefreshToken>> GetByUserIdAsync(Guid userId)
        {
            // Step 1: Filter the RefreshTokens table to records whose UserId matches,
            //         then materialise the result into a list
            return _context.RefreshTokens
                .Where(x => x.UserId == userId)
                .ToListAsync();
        }

        /// <summary>
        /// Updates an existing refresh token in the database.
        /// Typically used to mark a token as revoked (IsRevoked = true).
        /// </summary>
        /// <param name="token">The RefreshToken entity with updated values.</param>
        public async Task UpdateAsync(RefreshToken token)
        {
            // Step 1: Mark the RefreshToken entity as modified in the DbContext change tracker
            _context.RefreshTokens.Update(token);

            // Step 2: Commit the change to the database (executes UPDATE)
            await _context.SaveChangesAsync();
        }
    }
}
