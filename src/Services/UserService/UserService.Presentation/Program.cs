using UserService.Infrastructure.Persistence;
using UserService.Application;
using UserService.Infrastructure;
using UserService.Presentation;
using UserService.Presentation.Middlewares;
using Microsoft.EntityFrameworkCore;
using Telemetry;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPresentation(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure();

builder.Services.AddTelemetry(builder.Configuration);

builder.Logging.AddConsole();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<AppDbContext>();
    if (dbContext.Database.IsRelational())
    {
        dbContext.Database.Migrate();
    }
}

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapTelemetry();

app.Run();
