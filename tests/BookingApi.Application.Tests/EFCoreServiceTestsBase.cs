using BookingApi.Application.Interfaces;
using BookingApi.Application.Services;
using BookingApi.Infrastructure.Persistence;
using BookingApi.Infrastructure.Repositories;
using BookingApi.Infrastructure.Security;
using BookingApi.Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookingApi.Application.Tests;

public abstract class EFCoreServiceTestsBase : IDisposable
{
    protected readonly ServiceProvider _serviceProvider;
    protected readonly IServiceScope _scope;
    protected readonly IEventService _eventService;
    protected readonly IBookingService _bookingService;
    protected readonly IUserService _userService;
    protected readonly CancellationToken _ct;

    protected EFCoreServiceTestsBase()
    {
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(dbName));

        services.Configure<JwtSettings>(options =>
        {
            options.SigningKey = "TestSigningKeyWithAtLeast32CharactersLong!";
            options.Issuer = "TestIssuer";
            options.Audience = "TestAudience";
            options.ExpiresInMinutes = 60;
        });

        services.AddSingleton<IPasswordHasher, Sha256PasswordHasher>();
        services.AddSingleton<ITokenGenerator, JwtTokenGenerator>();

        services.AddScoped<IEventRepository, EFCoreEventRepository>();
        services.AddScoped<IBookingRepository, EFCoreBookingRepository>();
        services.AddScoped<IUserRepository, EFCoreUserRepository>();
        services.AddScoped<IUnitOfWork, EfCoreUnitOfWork>();

        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IUserService, UserService>();

        _serviceProvider = services.BuildServiceProvider();
        _scope = _serviceProvider.CreateScope();

        var context = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();

        _bookingService = _scope.ServiceProvider.GetRequiredService<IBookingService>();
        _eventService = _scope.ServiceProvider.GetRequiredService<IEventService>();
        _userService = _scope.ServiceProvider.GetRequiredService<IUserService>();

        _ct = CancellationToken.None;
    }

    public void Dispose()
    {
        _scope?.Dispose();
        _serviceProvider?.Dispose();
    }
}
