using CineVault.API.Extensions;
using Microsoft.AspNetCore.Mvc;
[assembly: ApiController]

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddCineVaultDbContext(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddRepositories();
builder.Services.AddApiVersioningWithApiExplorer();
builder.Services.AddSwaggerWithOptions();

var environment = builder.Environment.EnvironmentName;
Console.WriteLine($"=== Запуск у середовищі: {environment} ===");

var app = builder.Build();

if (app.Environment.IsLocal())
{
    app.UseDeveloperExceptionPage();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerWithOptions();
}

app.UseHttpsRedirection();
app.MapControllers();
await app.RunAsync();
