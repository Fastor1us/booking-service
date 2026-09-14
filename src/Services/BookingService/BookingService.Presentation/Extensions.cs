using BookingService.Infrastructure.Secure;
using Domain.Exceptions;
using Domain.Models;
using Messaging.Kafka;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

namespace BookingService.Presentation;

public static class Extensions
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        var configuration = services
            .BuildServiceProvider()
            .GetRequiredService<IConfiguration>();

        services.AddOptions<JwtSettings>()
            .Bind(configuration.GetSection("Jwt"))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(
                JwtBearerDefaults.AuthenticationScheme,
                options =>
                {
                    var serviceProvider = services.BuildServiceProvider();
                    var jwtSettings = serviceProvider
                        .GetRequiredService<IOptions<JwtSettings>>().Value;

                    options.TokenValidationParameters = new()
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwtSettings.Audience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        NameClaimType = ClaimTypes.NameIdentifier,
                        RoleClaimType = ClaimTypes.Role,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSettings.SigningKey))
                    };
                });

        services.Configure<KafkaOptions>(configuration.GetSection("Kafka"));

        services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage);

                    var errorMessage = "One or more validation errors occurred.";
                    throw new ModelValidationException(errorMessage, errors);
                };
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\""
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
        });

        return services;
    }

    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(value, out var userId))
        {
            throw new UnauthorizedAccessException("User identifier claim is missing.");
        }

        return userId;
    }

    public static UserRole GetUserRole(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.Role);

        if (!Enum.TryParse<UserRole>(value, ignoreCase: true, out var role))
        {
            throw new UnauthorizedAccessException("User role claim is missing or invalid.");
        }

        return role;
    }
}
