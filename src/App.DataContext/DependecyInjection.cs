using App.Application.Repositories;
using App.Application.Services;
using App.DataContext.Models;
using App.DataContext.Repositories;
using App.Domain;
using App.Domain.Enums;
using Microsoft.AspNetCore.Builder;
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

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserService, UserRepository>();
        services.AddScoped<UserManager<DataContext.Models.User>>();

        return services;
    }

    public static IServiceCollection AddIdentity(this IServiceCollection services)
    {
        services.AddIdentityCore<User>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddSignInManager<SignInManager<User>>()
            .AddDefaultTokenProviders();

        return services;
    }

    public static void ConfigureIdentityRoles(this IApplicationBuilder app)
    {
        using (var scope = app.ApplicationServices.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            dbContext.Database.Migrate();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            var roleNames = Enum.GetNames(typeof(UserRoles));
            foreach (var roleName in roleNames)
            {
                if (!roleManager.RoleExistsAsync(roleName).Result)
                {
                    roleManager.CreateAsync(new IdentityRole<Guid>(roleName)).Wait();
                }
            }
        }
    }
}
