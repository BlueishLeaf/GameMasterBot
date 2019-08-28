using System;
using System.Threading.Tasks;
using Common.Interfaces.DataAccess;
using DataAccess;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GameMasterBot.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GameMasterBot
{
    internal static class Program
    {
        private static void Main() => MainAsync().GetAwaiter().GetResult();

        private static async Task MainAsync()
        {
            using (var services = BuildServiceProvider())
            {
                var client = services.GetRequiredService<DiscordSocketClient>();
                client.Log += LogAsync;

                await client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DISCORD_DEV_TOKEN"));
                await client.StartAsync();

                await services.GetRequiredService<CommandHandler>().InitializeAsync();
                services.GetRequiredService<SessionService>().Initialize();

                await Task.Delay(-1);
            }
        }

        private static ServiceProvider BuildServiceProvider() => new ServiceCollection()
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<CommandService>()
            .AddSingleton<CommandHandler>()
            .AddSingleton<GameMasterContext>()
            .AddSingleton<IUnitOfWork, UnitOfWork>()
            .AddSingleton<CampaignService>()
            .AddScoped<SessionService>()
            .BuildServiceProvider();

        private static Task LogAsync(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
