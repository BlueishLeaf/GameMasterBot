using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using GameMasterBot.Data;
using GameMasterBot.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GameMasterBot
{
    internal class Program
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactionService;

        private Program()
        {
            var clientConfig = new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged
            };
            _client = new DiscordSocketClient(clientConfig);
            _interactionService = new InteractionService(_client.Rest);
        }
        
        private static Task Main() => new Program().MainAsync();

        private async Task MainAsync()
        {
            await using var services = BuildServiceProvider();

            await services.GetRequiredService<GameMasterBotContext>().Database.MigrateAsync();

            _client.Log += LogAsync;
            _interactionService.Log += LogAsync;
            _client.Ready += ReadyAsync;

            var discordToken = Environment.GetEnvironmentVariable("DISCORD_TOKEN");
            await _client.LoginAsync(TokenType.Bot, discordToken);
            await _client.StartAsync();

            await services.GetRequiredService<InteractionHandler>().InitializeAsync();

            await Task.Delay(Timeout.Infinite);
        }

        private ServiceProvider BuildServiceProvider() => new ServiceCollection()
            .AddSingleton(_client)
            .AddSingleton(_interactionService)
            .AddSingleton<InteractionHandler>()
            .AddDbContext<GameMasterBotContext>()
            .AddSingleton<UserService>()
            .AddSingleton<CampaignService>()
            .AddSingleton<SessionService>()
            .BuildServiceProvider();
        
        private static Task LogAsync(LogMessage logMessage)
        {
            Console.WriteLine(logMessage.ToString());
            return Task.CompletedTask;
        }

        private async Task ReadyAsync()
        {
            if (Debugger.IsAttached)
            {
                // Add the commands to a specific test Guild immediately
                var testGuildId = Environment.GetEnvironmentVariable("TEST_GUILD_ID");
                Console.WriteLine($"In debug mode, adding commands to {testGuildId}...");
                await _interactionService.RegisterCommandsToGuildAsync(Convert.ToUInt64(testGuildId));
            }
            else
            {
                // Add the commands globally, will take around an hour
                await _interactionService.RegisterCommandsGloballyAsync();
            }
            Console.WriteLine($"Connected as -> [{_client.CurrentUser}]");
            Console.WriteLine($"We are on [{_client.Guilds.Count}] guilds");
        }
    }
}
