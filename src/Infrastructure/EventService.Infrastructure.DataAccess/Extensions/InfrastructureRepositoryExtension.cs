using EventService.Application.Abstractions.Repositories;
using EventService.Infrastructure.DataAccess.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace EventService.Infrastructure.DataAccess.Extensions;

public static class InfrastructureRepositoryExtension
{
    public static IServiceCollection AddInfrastructureRepositories(this IServiceCollection services)
    {
        services.AddScoped<IArtistRepository, ArtistRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IEventOrganizerRepository, EventOrganizerRepository>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IHallSchemeRepository, HallSchemeRepository>();
        services.AddScoped<IOrganizerRepository, OrganizerRepository>();
        services.AddScoped<IVenueRepository, VenueRepository>();

        return services;
    }
}