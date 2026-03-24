using Microsoft.Extensions.Hosting;

namespace CineVault.API.Extensions;

// Клас розширень для IHostEnvironment
public static class IHostEnvironmentExtensions
{
    // Метод розширення IsLocal() перевіряє, чи середовище виконання локальне
    public static bool IsLocal(this IHostEnvironment env)
        => env.EnvironmentName.Equals("Local", StringComparison.OrdinalIgnoreCase);
}