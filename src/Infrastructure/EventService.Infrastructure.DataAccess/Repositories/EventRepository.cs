using EventService.Application.Abstractions.Repositories;
using EventService.Application.Models.Artists;
using EventService.Application.Models.Categories;
using EventService.Application.Models.EventEntities;
using EventService.Application.Models.Organizers;
using EventService.Application.Models.Venues;
using EventService.Infrastructure.DataAccess.DataBase.Options;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace EventService.Infrastructure.DataAccess.Repositories;

public class EventRepository : IEventRepository
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IVenueRepository _venueRepository;
    private readonly IEventOrganizerRepository _eventOrganizerRepository;
    private readonly IArtistRepository _artistRepository;
    private readonly NpgsqlDataSource _dataSource;

    public EventRepository(
        IOptions<DatabaseOptions> options,
        ICategoryRepository categoryRepository,
        IVenueRepository venueRepository,
        IEventOrganizerRepository eventOrganizerRepository,
        IArtistRepository artistRepository)
    {
        var builder = new NpgsqlDataSourceBuilder(options.Value.GetConnectionString());
        _dataSource = builder.Build();

        _categoryRepository = categoryRepository;
        _venueRepository = venueRepository;
        _eventOrganizerRepository = eventOrganizerRepository;
        _artistRepository = artistRepository;
    }

    public async Task<long> AddAsync(EventEntity entity, CancellationToken cancellationToken)
    {
        const string sql = @"
INSERT INTO events (title, description, start_date, end_date, category_id, venue_id)
VALUES (@title, @desc, @start, @end, @catId, @venueId) RETURNING id;";

        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("title", entity.Title);
        cmd.Parameters.AddWithValue("desc", entity.Description);
        cmd.Parameters.AddWithValue("start", entity.StartDate);
        cmd.Parameters.AddWithValue("end", entity.EndDate);
        cmd.Parameters.AddWithValue("catId", entity.CategoryId);
        cmd.Parameters.AddWithValue("venueId", entity.VenueId);

        long id = (long)(await cmd.ExecuteScalarAsync(cancellationToken) ?? 0);

        return id;
    }

    public async Task UpdateAsync(EventEntity entity, CancellationToken cancellationToken)
    {
        const string sql = @"
UPDATE events SET title=@title, description=@desc, start_date=@start, end_date=@end,
category_id=@catId, venue_id=@venueId WHERE id=@id;";

        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("title", entity.Title);
        cmd.Parameters.AddWithValue("desc", entity.Description);
        cmd.Parameters.AddWithValue("start", entity.StartDate);
        cmd.Parameters.AddWithValue("end", entity.EndDate);
        cmd.Parameters.AddWithValue("catId", entity.CategoryId);
        cmd.Parameters.AddWithValue("venueId", entity.VenueId);
        cmd.Parameters.AddWithValue("id", entity.Id);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task DeleteAsync(long id, CancellationToken cancellationToken)
    {
        const string sql = "DELETE FROM events WHERE id=@id;";

        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(long eventId, CancellationToken cancellationToken)
    {
        const string sql = "SELECT 1 FROM events WHERE id=@id;";

        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", eventId);

        object? result = await cmd.ExecuteScalarAsync(cancellationToken);
        return result != null;
    }

    public async Task<EventEntity?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT id, title, description, start_date, end_date, category_id, venue_id 
FROM events WHERE id=@id;";

        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("id", id);

        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        long eventId = reader.GetInt64(0);
        string title = reader.GetString(1);
        string desc = reader.GetString(2);
        DateTime start = reader.GetDateTime(3);
        DateTime end = reader.GetDateTime(4);
        long catId = reader.GetInt64(5);
        long venueId = reader.GetInt64(6);

        Category? category = await _categoryRepository.GetByIdAsync(catId, cancellationToken);
        Venue? venue = await _venueRepository.GetByIdAsync(venueId, cancellationToken);

        if (category == null || venue == null)
            return null;

        var organizers = new Collection<EventOrganizer>();
        await foreach (EventOrganizer org in _eventOrganizerRepository.GetByEventAsync(eventId, cancellationToken))
            organizers.Add(org);

        var artists = new Collection<Artist>();
        await foreach (Artist artist in _artistRepository.GetByEventAsync(eventId, cancellationToken))
            artists.Add(artist);

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

    public async IAsyncEnumerable<EventEntity> GetAllAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string sql = @"
SELECT id, title, description, start_date, end_date, category_id, venue_id 
FROM events;";

        await using NpgsqlConnection conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var cmd = new NpgsqlCommand(sql, conn);
        await using NpgsqlDataReader reader = await cmd.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            long eventId = reader.GetInt64(0);
            string title = reader.GetString(1);
            string desc = reader.GetString(2);
            DateTime start = reader.GetDateTime(3);
            DateTime end = reader.GetDateTime(4);
            long catId = reader.GetInt64(5);
            long venueId = reader.GetInt64(6);

            Category? category = await _categoryRepository.GetByIdAsync(catId, cancellationToken);
            Venue? venue = await _venueRepository.GetByIdAsync(venueId, cancellationToken);

            if (category == null || venue == null)
                continue;

            var organizers = new Collection<EventOrganizer>();
            await foreach (EventOrganizer org in _eventOrganizerRepository.GetByEventAsync(eventId, cancellationToken))
                organizers.Add(org);

            var artists = new Collection<Artist>();
            await foreach (Artist artist in _artistRepository.GetByEventAsync(eventId, cancellationToken))
                artists.Add(artist);

            yield return new EventEntity(
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
    }

    public async Task<IReadOnlyList<EventEntity>> GetByCategoryAsync(
        long categoryId,
        CancellationToken cancellationToken)
    {
        var result = new Collection<EventEntity>();
        await foreach (EventEntity e in GetAllAsync(cancellationToken))
        {
            if (e.CategoryId == categoryId)
                result.Add(e);
        }

        return result;
    }

    public async Task<IReadOnlyList<EventEntity>> GetByVenueAsync(long venueId, CancellationToken cancellationToken)
    {
        var result = new Collection<EventEntity>();
        await foreach (EventEntity e in GetAllAsync(cancellationToken))
        {
            if (e.VenueId == venueId)
                result.Add(e);
        }

        return result;
    }

    public async Task<IReadOnlyList<EventEntity>> GetByDateRangeAsync(
        DateTime left,
        DateTime right,
        CancellationToken cancellationToken)
    {
        var result = new Collection<EventEntity>();
        await foreach (EventEntity e in GetAllAsync(cancellationToken))
        {
            if (e.StartDate >= left && e.EndDate <= right)
                result.Add(e);
        }

        return result;
    }
}