using EventService.Application.Contracts.EventManagerServices;
using EventService.Application.Contracts.SeatValidationServices;
using EventService.Application.Contracts.VenueManagementServices;
using EventService.Application.EventManagerServices;
using EventService.Application.SeatValidationServices;
using EventService.Application.VenueManagementServices;
using Microsoft.Extensions.DependencyInjection;

namespace EventService.Application.Extensions;

public static class ApplicationServiceExtension
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IEventManagerService, EventManagerService>();
        services.AddScoped<ISeatValidationService, SeatValidationService>();
        services.AddScoped<IVenueManagementService, VenueManagementService>();

        return services;
    }
}