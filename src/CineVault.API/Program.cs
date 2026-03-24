using CineVault.API.Extensions;
using Microsoft.AspNetCore.Mvc;

[assembly: ApiController]

var builder = WebApplication.CreateBuilder(args);

// Налаштування сервісів: БД, контролери, репозиторії, Swagger
builder.Services.AddCineVaultDbContext(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddRepositories();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Вивід активного середовища у консоль
var environment = builder.Environment.EnvironmentName;
Console.WriteLine($"=== Запуск у середовищі: {environment} ===");

var app = builder.Build();

// Використання спеціальних налаштувань для Local та Development
if (app.Environment.IsLocal())
{
    app.UseDeveloperExceptionPage();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Основна маршрутизація та HTTPS
app.UseHttpsRedirection();
app.MapControllers();

// Запуск застосунку
await app.RunAsync();
