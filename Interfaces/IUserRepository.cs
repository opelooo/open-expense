using AccountingApp.Models;

namespace AccountingApp.Interfaces
{
    /// <summary>
    /// User-specific repository interface
    /// </summary>
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetUserByUsernameAsync(string username);
        Task<bool> IsUserExistsAsync(string username);
        Task<string?> GetIdByUsernameAsync(string username);
    }
}
