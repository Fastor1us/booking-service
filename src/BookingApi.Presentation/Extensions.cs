using System.Text;
using System.Text.Json.Serialization;
using BookingApi.Domain.Exceptions;
using BookingApi.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BookingApi.Presentation;

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
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSettings.SigningKey))
                    };
                });

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

        services.AddSwaggerGen();

        return services;
    }
}
