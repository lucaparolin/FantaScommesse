using FantaScommesse.Models.Entities;

namespace FantaScommesse.Repositories;

/// <summary>
/// Repository interface for User entity operations
/// </summary>
public interface IUserRepository : IRepository<User, long>
{
    Task<User?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
}
