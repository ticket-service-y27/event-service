namespace EventService.Application.Abstractions.Repositories;

public interface IRepository<T>
{
    Task<T?> GetByIdAsync(long id);
    
    Task<IReadOnlyList<T>> GetAllAsync();
    
    Task AddAsync(T entity);
    
    Task UpdateAsync(T entity);
    
    Task DeleteAsync(long id);
}