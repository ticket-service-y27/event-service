using EventService.Application.Abstractions.Repositories;
using EventService.Application.Models.Artists;
using EventService.Application.Models.Categories;
using EventService.Application.Models.Events;
using EventService.Application.Models.Organizers;
using EventService.Application.Models.Venues;
using Npgsql;
using System.Collections.ObjectModel;

namespace EventService.Infrastructure.DataAccess.Repositories;

public class EventRepository : IEventRepository
{
    private readonly string _connectionString;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IVenueRepository _venueRepository;
    private readonly IEventOrganizerRepository _eventOrganizerRepository;
    private readonly IArtistRepository _artistRepository;

    public EventRepository(
        string connectionString,
        ICategoryRepository categoryRepository,
        IVenueRepository venueRepository,
        IEventOrganizerRepository eventOrganizerRepository,
        IArtistRepository artistRepository)
    {
        _connectionString = connectionString;
        _categoryRepository = categoryRepository;
        _venueRepository = venueRepository;
        _eventOrganizerRepository = eventOrganizerRepository;
        _artistRepository = artistRepository;
    }

    public async Task AddAsync(EventEntity entity)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            @"INSERT INTO events 
                  (title, description, start_date, end_date, category_id, venue_id)
                  VALUES (@title, @desc, @start, @end, @catId, @venueId) RETURNING id",
            conn);

        cmd.Parameters.AddWithValue("title", entity.Title);
        cmd.Parameters.AddWithValue("desc", entity.Description);
        cmd.Parameters.AddWithValue("start", entity.StartDate);
        cmd.Parameters.AddWithValue("end", entity.EndDate);
        cmd.Parameters.AddWithValue("catId", entity.CategoryId);
        cmd.Parameters.AddWithValue("venueId", entity.VenueId);

        object? result = await cmd.ExecuteScalarAsync();

        if (result == null)
            throw new ArgumentException($"Could not insert event with id {entity.Id}");

        long id = (long)result;

        typeof(EventEntity).GetProperty("Id")?.SetValue(entity, id);
    }

    public async Task DeleteAsync(long id)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("DELETE FROM events WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<bool> ExistsAsync(long eventId)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("SELECT 1 FROM events WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", eventId);

        object? result = await cmd.ExecuteScalarAsync();
        return result != null;
    }

    public async Task<IReadOnlyList<EventEntity>> GetAllAsync()
    {
        var result = new Collection<EventEntity>();
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand("SELECT id, title, description, start_date, end_date, category_id, venue_id FROM events", conn);
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            long id = reader.GetInt64(0);
            string title = reader.GetString(1);
            string desc = reader.GetString(2);
            DateTime start = reader.GetDateTime(3);
            DateTime end = reader.GetDateTime(4);
            long catId = reader.GetInt64(5);
            long venueId = reader.GetInt64(6);

            Category? category = await _categoryRepository.GetByIdAsync(catId);
            Venue? venue = await _venueRepository.GetByIdAsync(venueId);
            var organizers = new Collection<EventOrganizer>((IList<EventOrganizer>)await _eventOrganizerRepository.GetByEventAsync(id));
            var artists = new Collection<Artist>((IList<Artist>)await _artistRepository.GetByEventAsync(id));

            if (category != null && venue != null)
            {
                result.Add(new EventEntity(
                    Id: id,
                    Title: title,
                    Description: desc,
                    StartDate: start,
                    EndDate: end,
                    CategoryId: catId,
                    Category: category,
                    VenueId: venueId,
                    Venue: venue,
                    Organizers: organizers,
                    Artists: artists));
            }
        }

        return result;
    }

    public async Task<EventEntity?> GetByIdAsync(long id)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            "SELECT id, title, description, start_date, end_date, category_id, venue_id FROM events WHERE id = @id", conn);
        cmd.Parameters.AddWithValue("id", id);

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return null;

        long eventId = reader.GetInt64(0);
        string title = reader.GetString(1);
        string desc = reader.GetString(2);
        DateTime start = reader.GetDateTime(3);
        DateTime end = reader.GetDateTime(4);
        long catId = reader.GetInt64(5);
        long venueId = reader.GetInt64(6);

        Category? category = await _categoryRepository.GetByIdAsync(catId);
        Venue? venue = await _venueRepository.GetByIdAsync(venueId);
        var organizers = new Collection<EventOrganizer>((IList<EventOrganizer>)await _eventOrganizerRepository.GetByEventAsync(eventId));
        var artists = new Collection<Artist>((IList<Artist>)await _artistRepository.GetByEventAsync(eventId));

        if (category == null || venue == null)
            return null;

        return new EventEntity(
            Id: eventId,
            Title: title,
            Description: desc,
            StartDate: start,
            EndDate: end,
            CategoryId: catId,
            Category: category,
            VenueId: venueId,
            Venue: venue,
            Organizers: organizers,
            Artists: artists);
    }

    public async Task<IReadOnlyList<EventEntity>> GetByCategoryAsync(long categoryId)
    {
        IReadOnlyList<EventEntity> allEvents = await GetAllAsync();
        return allEvents.Where(e => e.CategoryId == categoryId).ToList();
    }

    public async Task<IReadOnlyList<EventEntity>> GetByVenueAsync(long venueId)
    {
        IReadOnlyList<EventEntity> allEvents = await GetAllAsync();
        return allEvents.Where(e => e.VenueId == venueId).ToList();
    }

    public async Task<IReadOnlyList<EventEntity>> GetByDateRangeAsync(DateTime left, DateTime right)
    {
        IReadOnlyList<EventEntity> allEvents = await GetAllAsync();
        return allEvents.Where(e => e.StartDate >= left && e.EndDate <= right).ToList();
    }

    public async Task UpdateAsync(EventEntity entity)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            @"UPDATE events SET title=@title, description=@desc, start_date=@start, end_date=@end,
                  category_id=@catId, venue_id=@venueId WHERE id=@id",
            conn);

        cmd.Parameters.AddWithValue("title", entity.Title);
        cmd.Parameters.AddWithValue("desc", entity.Description);
        cmd.Parameters.AddWithValue("start", entity.StartDate);
        cmd.Parameters.AddWithValue("end", entity.EndDate);
        cmd.Parameters.AddWithValue("catId", entity.CategoryId);
        cmd.Parameters.AddWithValue("venueId", entity.VenueId);
        cmd.Parameters.AddWithValue("id", entity.Id);

        await cmd.ExecuteNonQueryAsync();
    }
}