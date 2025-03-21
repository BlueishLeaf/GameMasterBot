﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using GameMasterBot.Data;
using GameMasterBot.Services;
using GameMasterBot.Services.Interfaces;
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
                GatewayIntents = GatewayIntents.Guilds
            };
            _client = new DiscordSocketClient(clientConfig);
            _interactionService = new InteractionService(_client.Rest);
        }
        
        private static Task Main() => new Program().MainAsync();

        private async Task MainAsync()
        {
            await using var services = BuildServiceProvider();

            #if DEBUG
            await services.GetRequiredService<GameMasterBotContext>().Database.MigrateAsync();
            #endif

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
            .AddDbContext<GameMasterBotContext>()
            .AddSingleton(_client)
            .AddSingleton(_interactionService)
            .AddSingleton<InteractionHandler>()
            .AddSingleton<SessionScheduler>()
            .AddSingleton<CampaignCommandValidator>()
            .AddSingleton<SessionCommandValidator>()
            .AddSingleton<TimezoneCommandValidator>()
            .AddScoped<IUserService, UserService>()
            .AddScoped<ICampaignService, CampaignService>()
            .AddScoped<ISessionService, SessionService>()
            .AddScoped<ISessionSchedulingService, SessionSchedulingService>()
            .BuildServiceProvider();
        
        private static Task LogAsync(LogMessage logMessage)
        {
            Console.WriteLine(logMessage.ToString());
            return Task.CompletedTask;
        }

        private async Task ReadyAsync()
        {
            #if DEBUG
            // Add the commands to a specific test Guild immediately
            var testGuildId = Environment.GetEnvironmentVariable("TEST_GUILD_ID");
            Console.WriteLine($"{DateTime.Now:T} In debug mode, adding commands to {testGuildId}...");
            await _interactionService.RegisterCommandsToGuildAsync(Convert.ToUInt64(testGuildId));
            #else
            Console.WriteLine($"{DateTime.Now:T} In release mode, adding commands to all guilds...");
            await _interactionService.RegisterCommandsGloballyAsync();
            #endif
            Console.WriteLine($"{DateTime.Now:T} Connected as -> [{_client.CurrentUser}]");
            Console.WriteLine($"{DateTime.Now:T} I am on [{_client.Guilds.Count}] guilds");
        }
    }
}
