using EventService.Presentation.Grpc.Interceptors;
using EventService.Presentation.Grpc.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace EventService.Presentation.Grpc.Extensions;

public static class GrpcServiceCollectionExtension
{
    public static IServiceCollection AddGrpcServices(this IServiceCollection services)
    {
        services.AddScoped<GrpcExceptionInterceptor>();

        services.AddGrpc(options =>
        {
            options.Interceptors.Add<GrpcExceptionInterceptor>();
        });

        return services;
    }

    public static IEndpointRouteBuilder MapGrpcEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGrpcService<EventManagerServiceGrpc>();
        endpoints.MapGrpcService<SeatValidationServiceGrpc>();
        endpoints.MapGrpcService<VenueManagementServiceGrpc>();

        return endpoints;
    }
}