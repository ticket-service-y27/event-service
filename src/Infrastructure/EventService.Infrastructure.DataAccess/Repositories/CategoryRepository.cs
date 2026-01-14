using EventService.Application.Abstractions.Repositories;
using EventService.Application.Models.Categories;
using EventService.Infrastructure.DataAccess.Contexts;
using Microsoft.EntityFrameworkCore;

namespace EventService.Infrastructure.DataAccess.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    private readonly EventDbContext _context;

    public CategoryRepository(EventDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<Category?> GetByNameAsync(string name)
    {
        return await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
    }
}