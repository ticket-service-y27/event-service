using EventService.Application.Extensions;
using EventService.Infrastructure.DataAccess.Extensions;
using EventService.Presentation.Grpc.Extensions;
using FluentMigrator.Runner;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationServices();

builder.Services.AddDatabaseOptions(builder.Configuration)
    .AddNpgsqlDataSource()
    .AddMigrations();

builder.Services.AddInfrastructureRepositories();

builder.Services.AddGrpcServices();

WebApplication app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    IMigrationRunner runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    runner.MigrateUp();
}

app.MapGrpcEndpoints();

app.Run();