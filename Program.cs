using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GameMasterBot.Data;
using GameMasterBot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace GameMasterBot
{
    internal static class Program
    {
        private static void Main() => MainAsync().GetAwaiter().GetResult();

        private static async Task MainAsync()
        {
            await using var services = BuildServiceProvider();
            var client = services.GetRequiredService<DiscordSocketClient>();
            
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("logs/GameMasterBot.log", rollingInterval: RollingInterval.Day)
                .WriteTo.Console(theme: SystemConsoleTheme.Colored)
                .CreateLogger();
            services.GetRequiredService<LoggingService>();
            await client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DISCORD_TOKEN"));
            await client.StartAsync();
            
            await services.GetRequiredService<CommandHandler>().InitializeAsync();
            await services.GetRequiredService<GameMasterContext>().Database.MigrateAsync();

            await Task.Delay(-1);
        }

        private static ServiceProvider BuildServiceProvider() => new ServiceCollection()
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<CommandService>()
            .AddSingleton<CommandHandler>()
            .AddDbContext<GameMasterContext>()
            .AddSingleton<UserService>()
            .AddSingleton<CampaignService>()
            .AddSingleton<SessionService>()
            .AddSingleton<LoggingService>()
            .AddLogging(configure => configure.AddSerilog())
            .BuildServiceProvider();
    }
}
