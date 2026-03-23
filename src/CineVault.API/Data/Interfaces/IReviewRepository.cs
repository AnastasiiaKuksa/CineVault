using CineVault.API.Data.Entities;

namespace CineVault.API.Data.Interfaces;

public interface IReviewRepository
{
    Task<IReadOnlyList<Review>> GetAllWithDetails();
    Task<Review?> GetByIdWithDetails(int id);
    Task Create(Review review);
    Task Update(Review review);
    Task Delete(Review review);
}
