using EventService.Application.Models.Categories;

namespace EventService.Application.Abstractions.Repositories;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(long id);

    Task<IReadOnlyList<Category>> GetAllAsync();

    Task AddAsync(Category entity);

    Task UpdateAsync(Category entity);

    Task DeleteAsync(long id);

    Task<Category?> GetByNameAsync(string name);
}