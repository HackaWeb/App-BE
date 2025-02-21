using App.DataContext.Repository;
using App.Domain;
using App.Domain.Models;
using App.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
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

    public static IServiceCollection AddIdentity(this IServiceCollection services)
    {
        services.AddIdentity<User, IdentityRole<Guid>>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
        })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }
}
