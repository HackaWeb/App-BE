using App.Application.Services;
using App.Domain;
using App.Domain.Models;
using App.Domain.Settings;
using App.Infrastructure.Settings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace App.Api.Extensions;

public static class ServiceExtensions
{
    public static IServiceCollection AddJwt(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection(nameof(JwtSettings));
        var key = Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable(AppConstants.JWT_SECRET) ?? jwtSettings[nameof(JwtSettings.Secret)]!);

        var googleSettings = configuration.GetSection(nameof(GoogleAuthenticationSettings));
        var googleClientId = Environment.GetEnvironmentVariable(AppConstants.GOOGLE_CLIENT_ID) ?? googleSettings[nameof(GoogleAuthenticationSettings.ClientId)]!;
        var googleClientSecret = Environment.GetEnvironmentVariable(AppConstants.GOOGLE_CLIENT_SECRET) ?? googleSettings[nameof(GoogleAuthenticationSettings.ClientSecret)]!;

        var githubSettings = configuration.GetSection(nameof(GithubAuthenticationSettings));
        var githubClientId = Environment.GetEnvironmentVariable(AppConstants.GITHUB_CLIENT_ID) ?? githubSettings[nameof(GithubAuthenticationSettings.ClientId)]!;
        var githubClientSecret = Environment.GetEnvironmentVariable(AppConstants.GITHUB_CLIENT_SECRET) ?? githubSettings[nameof(GithubAuthenticationSettings.ClientSecret)]!;

        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        services.AddAuthentication(options =>
        {
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
        })
        .AddGoogle(googleOptions =>
        {
            googleOptions.ClientId = googleClientId;
            googleOptions.ClientSecret = googleClientSecret;
            googleOptions.CallbackPath = "/signin-google";
        })
        .AddOAuth("GitHub", githubOptions =>
        {
            githubOptions.ClientId = githubClientId;
            githubOptions.ClientSecret = githubClientSecret;
            githubOptions.CallbackPath = "/signin-github";
            githubOptions.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
            githubOptions.TokenEndpoint = "https://github.com/login/oauth/access_token";
            githubOptions.UserInformationEndpoint = "https://api.github.com/user";

            githubOptions.Scope.Add("user:email");
            githubOptions.Scope.Add("read:user");

            githubOptions.ClaimActions.MapJsonKey(ClaimTypes.GivenName, "login");
            githubOptions.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");

            githubOptions.SaveTokens = true;

            githubOptions.Events = new OAuthEvents
            {
                OnCreatingTicket = async context =>
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user/emails");
                    request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", context.AccessToken);
                    request.Headers.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("MyApp", "1.0"));

                    var response = await context.Backchannel.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        using var jsonDoc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                        var emailElement = jsonDoc.RootElement.EnumerateArray()
                            .FirstOrDefault(e => e.TryGetProperty("primary", out var primary) && primary.GetBoolean());

                        if (emailElement.TryGetProperty("email", out var email))
                        {
                            context.Identity?.AddClaim(new Claim(ClaimTypes.Email, email.GetString() ?? ""));
                        }
                    }
                }
            };
        });

        services.AddAuthorization();
        return services;
    } 

    public static IServiceCollection ConfigureMediatR(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Application.AssemblyReference.Assembly));

        services.AddHttpContextAccessor();
        services.AddScoped<SignInManager<User>>();
        services.AddScoped<UserManager<User>>();

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions();
        services.AddLogging();

        services.Configure<JwtSettings>(configuration.GetSection(nameof(JwtSettings)));
        services.Configure<GoogleAuthenticationSettings>(configuration.GetSection(nameof(GoogleAuthenticationSettings)));

        services.AddHttpContextAccessor();

        return services;
    }

    public static IServiceCollection AddCorsPolicies(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
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
