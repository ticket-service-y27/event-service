using EventService.Application.Abstractions.Messaging;
using EventService.Presentation.Kafka.Options;
using EventService.Presentation.Kafka.Publishers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EventService.Presentation.Kafka.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddKafka(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KafkaOptions>(
            configuration.GetSection("Kafka"));

        services.AddScoped<IEventCreatedPublisher, EventCreatedPublisher>();
        services.AddScoped<ISeatBookedPublisher, SeatBookedPublisher>();
        services.AddScoped<ISeatReturnedPublisher, SeatReturnedPublisher>();
        services.AddScoped<IVenueCreatedPublisher, VenueCreatedPublisher>();
        return services;
    }
}