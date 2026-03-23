using CineVault.API.Data.Entities;

namespace CineVault.API.Data.Interfaces;

public interface IUserRepository
{
    Task<IReadOnlyList<User>> GetAll();
    Task<User?> GetById(int id);
    Task Create(User user);
    Task Update(User user);
    Task Delete(User user);
}