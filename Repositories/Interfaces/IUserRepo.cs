using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repositories.Models;
namespace Repositories.Interfaces
{
    public interface IUserRepo
    {
        Task<(List<User> Items, int TotalCount)> GetAllUsersAsync(int pageNumber, int pageSize, bool includeLocked = false);
        Task<User?> GetUserByIdAsync(Guid id, bool includeLocked = false);

        Task<User?> GetUserByEmailAsync(string email);
        Task CreateUserAsync(User user);
        Task UpdateUserAsync(User user);
    }
}
