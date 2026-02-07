using AccountingApp.Data;
using AccountingApp.Interfaces;
using AccountingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountingApp.Repositories
{
    /// <summary>
    /// User-specific repository implementation
    /// </summary>
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext dbContext)
            : base(dbContext) { }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            return await FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<bool> IsUserExistsAsync(string username)
        {
            return await AnyAsync(u => u.Username == username);
        }

        public async Task<string?> GetIdByUsernameAsync(string username)
        {
            var user = await FirstOrDefaultAsync(u => u.Username == username);
            return user?.Id;
        }
    }
}
