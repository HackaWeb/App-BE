using App.Application.Handlers.Slack;
using App.Application.Repositories;
using App.Application.Services;
using App.DataContext.Mapping;
using App.Domain;
using App.Domain.Enums;
using App.Domain.Settings;
using App.Infrastructure.Repository;
using App.Infrastructure.Services;
using App.Infrastructure.Settings;
using AutoMapper.Extensions.ExpressionMapping;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace App.Api.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddJwtSettings(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection(nameof(JwtSettings));
        var key = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable(AppConstants.JWT_SECRET) ?? jwtSettings[nameof(JwtSettings.Secret)]!);

        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings[nameof(JwtSettings.Issuer)],
                    ValidAudience = jwtSettings[nameof(JwtSettings.Audience)],
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("Admin", policy =>
            {
                policy.RequireRole(nameof(UserRoles.ADMIN));
            });
        });

        return services;
    } 

    public static IServiceCollection ConfigureMediatR(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Application.AssemblyReference.Assembly));
        services.AddHttpContextAccessor();

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions();
        services.AddLogging();

        services.AddScoped<IBlobStorageRepository, BlobStorageRepository>();
        services.AddScoped<IOpenAIService, OpenAIService>();

        services.Configure<JwtSettings>(configuration.GetSection(nameof(JwtSettings)));
        services.Configure<OpenAISettings>(configuration.GetSection(nameof(OpenAISettings)));
        services.Configure<TrelloSettings>(configuration.GetSection(nameof(TrelloSettings)));
        services.Configure<SlackSettings>(configuration.GetSection(nameof(SlackSettings)));

        services.AddAutoMapper(cfg =>
        {
            cfg.AddProfile<MapperProfiles>();
            cfg.AddExpressionMapping();
        });

        services.AddHttpClient<IOpenAIService, OpenAIService>(client =>
        {
            client.BaseAddress = new Uri("https://api.openai.com/");
        });

        services.AddHttpContextAccessor();

        return services;
    }

    public static IServiceCollection AddCorsPolicies(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll",
                policy => policy.SetIsOriginAllowed(_ => true)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials());
        });

        return services;
    }

    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "App API",
                Version = "v1"
            });

            options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Description = "Enter JWT token"
            });

            options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
}
