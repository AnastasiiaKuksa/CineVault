using CineVault.API.Common.Mappings;
using CineVault.API.BackgroundServices;
using CineVault.API.Data.Entities;
using CineVault.API.Extensions;
using CineVault.API.Middleware;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[assembly: ApiController]

var builder = WebApplication.CreateBuilder(args);
builder.Host.AddLogging();
builder.Logging.ClearProviders();
builder.Services.AddCineVaultDbContext(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddRepositories();
builder.Services.AddApiVersioningWithApiExplorer();
builder.Services.AddSwaggerWithOptions();
builder.Services.AddMapster(typeof(Program));

builder.Services.AddMemoryCache();

builder.Services.AddDistributedSqlServerCache(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("CineVaultDb");
    options.SchemaName = "dbo";
    options.TableName = "CacheTable";
});



// 
builder.Services.AddHostedService<MovieStatsUpdaterService>();

// 
builder.Services.AddHostedService<OldMoviesCleanerService>();

var environment = builder.Environment.EnvironmentName;


Console.WriteLine($"=== Запуск у середовищі: {environment} ===");


var app = builder.Build();

// Auto-create database and apply migrations
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CineVaultDbContext>();
    // Apply pending migrations (creates DB if not exists)
    await dbContext.Database.MigrateAsync();
}

if (app.Environment.IsLocal())
{
    app.UseDeveloperExceptionPage();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerWithOptions();
}

app.UseMiddleware<PerformanceLoggingMiddleware>();
app.UseHttpsRedirection();
app.MapControllers();

await app.RunAsync();
