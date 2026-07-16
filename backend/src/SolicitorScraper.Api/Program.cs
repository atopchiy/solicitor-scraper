using Microsoft.EntityFrameworkCore;
using SolicitorScraper.Api.Middleware;
using SolicitorScraper.Application;
using SolicitorScraper.Infrastructure;
using SolicitorScraper.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy => policy
        .WithOrigins("http://localhost:4200")
        .AllowAnyHeader()
        .AllowAnyMethod()));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
}

app.UseCors();
app.UseMiddleware<ApiExceptionMiddleware>();
app.MapControllers();

app.Run();
