using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GameMasterBot.Services
{
    public class LoggingService
    {
        private readonly ILogger _logger;
        private readonly DiscordSocketClient _discord;

        public LoggingService(IServiceProvider services)
        { 
            _discord = services.GetRequiredService<DiscordSocketClient>(); 
            CommandService commands = services.GetRequiredService<CommandService>();
            _logger = services.GetRequiredService<ILogger<LoggingService>>();
            _discord.Ready += OnReadyAsync;
            _discord.Log += OnLogAsync;
            commands.Log += OnLogAsync;
        }

        private Task OnReadyAsync()
        {
            _logger.LogInformation($"Connected as -> [{_discord.CurrentUser}]");
            _logger.LogInformation($"We are on [{_discord.Guilds.Count}] servers");
            return Task.CompletedTask;
        }

        private Task OnLogAsync(LogMessage msg)
        { 
            string logText = $"{msg.Source}: {msg.Exception?.ToString() ?? msg.Message}";
            switch (msg.Severity)
            {
                case LogSeverity.Critical:
                {
                    _logger.LogCritical(logText);
                    break;
                }
                case LogSeverity.Warning:
                {
                    _logger.LogWarning(logText);
                    break;
                }
                case LogSeverity.Info:
                {
                    _logger.LogInformation(logText);
                    break;
                }
                case LogSeverity.Verbose:
                {
                    _logger.LogInformation(logText);
                    break;
                } 
                case LogSeverity.Debug:
                {
                    _logger.LogDebug(logText);
                    break;
                } 
                case LogSeverity.Error:
                {
                    _logger.LogError(logText);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return Task.CompletedTask; 

        }
    }
}