using EventService.Application.Models.Categories;

namespace EventService.Application.Abstractions.Repositories;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(long id, CancellationToken cancellationToken);

    IAsyncEnumerable<Category> GetAllAsync(CancellationToken cancellationToken);

    Task AddAsync(Category entity, CancellationToken cancellationToken);

    Task UpdateAsync(Category entity, CancellationToken cancellationToken);

    Task DeleteAsync(long id, CancellationToken cancellationToken);

    Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken);
}