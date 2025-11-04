using Microsoft.EntityFrameworkCore;

namespace IMS_Backend;

public static class StartupTasks
{
    public static async Task StartupArgsHandler(string[] args, IServiceCollection serviceCollection)
    {
        if (args.Length == 0)
            return;
        
        var command = args[0].ToLowerInvariant();
        int? optionalValue = null;
        if (args.Length > 1 && int.TryParse(args[1], out var value))
            optionalValue = value;

        switch (command)
        {
            case "migrate-db":
                await RunScopedAsync(serviceCollection, MigrateDatabaseAsync);
                return;

            default:
                Console.WriteLine($"Unknown command: {command}");
                return;
        }
    }

    public static async Task RunScopedAsync(IServiceCollection serviceCollection, int? optionalInt, Func<ServiceProvider, int?, Task> action)
    {
        try
        {
            var services = serviceCollection.BuildServiceProvider();
            await action(services, optionalInt);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public static async Task RunScopedAsync(IServiceCollection serviceCollection, Func<ServiceProvider, Task> action)
    {
        try
        {
            var services = serviceCollection.BuildServiceProvider();
            await action(services);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public static async Task MigrateDatabaseAsync(ServiceProvider services)
    {
        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Console.WriteLine("Applying migrations...");
        await dbContext.Database.MigrateAsync();
        Console.WriteLine("Migrations applied.");
    }
}

