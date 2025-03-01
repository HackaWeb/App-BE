using App.DataContext.Repository;
using App.Domain;
using App.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace App.DataContext;

public static class DependecyInjection
{
    public static IServiceCollection AddDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionSettings = Environment.GetEnvironmentVariable(AppConstants.DB_CONNECTION_STRING)
            ?? configuration.GetConnectionString("AppConnection");

        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionSettings));

        services.AddScoped<IBlobStorageRepository, BlobStorageRepository>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        return services;
    }
}
