using EventService.Application.Abstractions.Repositories;
using EventService.Application.Models.Categories;
using Npgsql;
using System.Collections.ObjectModel;

namespace EventService.Infrastructure.DataAccess.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly string _connectionString;
    private readonly IEventRepository _eventRepository;

    public CategoryRepository(string connectionString, IEventRepository eventRepository)
    {
        _connectionString = connectionString;
        _eventRepository = eventRepository;
    }

    public async Task<Category?> GetByIdAsync(long id)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            "SELECT id, name FROM categories WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return null;

        long categoryId = reader.GetInt64(0);
        string name = reader.GetString(1);

        IReadOnlyList<Application.Models.Events.EventEntity> events = await _eventRepository.GetByCategoryAsync(categoryId);

        return new Category(
            Id: categoryId,
            Name: name,
            Events: events);
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync()
    {
        var result = new Collection<Category>();

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("SELECT id, name FROM categories", conn);

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            long categoryId = reader.GetInt64(0);
            string name = reader.GetString(1);

            IReadOnlyList<Application.Models.Events.EventEntity> events = await _eventRepository.GetByCategoryAsync(categoryId);

            result.Add(new Category(
                Id: categoryId,
                Name: name,
                Events: events));
        }

        return result;
    }

    public async Task AddAsync(Category entity)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            "INSERT INTO categories (name) VALUES (@name) RETURNING id", conn);
        cmd.Parameters.AddWithValue("name", entity.Name);

        object? result = await cmd.ExecuteScalarAsync();

        if (result == null)
            throw new ArgumentException($"Could not insert category with id {entity.Id}");

        long id = (long)result;

        typeof(Category).GetProperty("Id")?.SetValue(entity, id);
    }

    public async Task UpdateAsync(Category entity)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            "UPDATE categories SET name = @name WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", entity.Id);
        cmd.Parameters.AddWithValue("name", entity.Name);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(long id)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("DELETE FROM categories WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<Category?> GetByNameAsync(string name)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            "SELECT id, name FROM categories WHERE name = @name", conn);
        cmd.Parameters.AddWithValue("name", name);

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return null;

        long categoryId = reader.GetInt64(0);

        IReadOnlyList<Application.Models.Events.EventEntity> events = await _eventRepository.GetByCategoryAsync(categoryId);

        return new Category(
            Id: categoryId,
            Name: reader.GetString(1),
            Events: events);
    }
}