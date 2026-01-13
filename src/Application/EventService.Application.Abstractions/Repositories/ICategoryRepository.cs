using EventService.Application.Models.Categories;

namespace EventService.Application.Abstractions.Repositories;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetByNameAsync(string name);
}