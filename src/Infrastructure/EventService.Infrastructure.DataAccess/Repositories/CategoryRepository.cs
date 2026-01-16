using EventService.Application.Abstractions.Repositories;
using EventService.Application.Models.Categories;
using EventService.Application.Models.EventEntities;
using EventService.Infrastructure.DataAccess.DataBase.Options;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Runtime.CompilerServices;

namespace EventService.Infrastructure.DataAccess.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly IEventRepository _eventRepository;

    public CategoryRepository(IOptions<DatabaseOptions> options, IEventRepository eventRepository)
    {
        var builder = new NpgsqlDataSourceBuilder(options.Value.GetConnectionString());
        _dataSource = builder.Build();
        _eventRepository = eventRepository;
    }

    public async Task<Category?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        const string sql = "SELECT id, name FROM categories WHERE id = @id";

        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        long categoryId = reader.GetInt64(0);
        string name = reader.GetString(1);

        IReadOnlyList<EventEntity> events = await _eventRepository.GetByCategoryAsync(categoryId, cancellationToken);

        return new Category(
            Id: categoryId,
            Name: name,
            Events: events);
    }

    public async IAsyncEnumerable<Category> GetAllAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string sql = "SELECT id, name FROM categories";

        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            long categoryId = reader.GetInt64(0);
            string name = reader.GetString(1);

            IReadOnlyList<EventEntity> events =
                await _eventRepository.GetByCategoryAsync(categoryId, cancellationToken);

            yield return new Category(
                Id: categoryId,
                Name: name,
                Events: events);
        }
    }

    public async Task<long> AddAsync(Category entity, CancellationToken cancellationToken = default)
    {
        const string sql = "INSERT INTO categories (name) VALUES (@name) RETURNING id";

        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("name", entity.Name);

        long id = (long)(await cmd.ExecuteScalarAsync(cancellationToken) ?? 0);

        return id;
    }

    public async Task UpdateAsync(Category entity, CancellationToken cancellationToken = default)
    {
        const string sql = "UPDATE categories SET name = @name WHERE id = @id";

        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", entity.Id);
        cmd.Parameters.AddWithValue("name", entity.Name);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        const string sql = "DELETE FROM categories WHERE id = @id";

        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<Category?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        const string sql = "SELECT id, name FROM categories WHERE name = @name";

        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("name", name);

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        long categoryId = reader.GetInt64(0);
        IReadOnlyList<EventEntity> events = await _eventRepository.GetByCategoryAsync(categoryId, cancellationToken);

        return new Category(
            Id: categoryId,
            Name: reader.GetString(1),
            Events: events);
    }
}