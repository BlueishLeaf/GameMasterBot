using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GameMasterBot.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GameMasterBot
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;
        private readonly ILogger _logger;

        public CommandHandler(DiscordSocketClient client, CommandService commands, IServiceProvider services)
        {
            _client = client;
            _commands = commands;
            _services = services;
            _logger = services.GetRequiredService<ILogger<CommandHandler>>();

            // Handle outcome of commands
            _commands.CommandExecuted += CommandExecutedAsync;

            // Handle commands
            _client.MessageReceived += HandleCommandAsync;
        }

        public async Task InitializeAsync() => await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            // Do not process command if it was a system message
            if (!(messageParam is SocketUserMessage message)) return;

            // Variable to track where prefix ends and command begins
            var argPos = 0;

            // Determine if message is a command and the sender is not a bot
            if (!(message.HasCharPrefix('!', ref argPos) ||
                  message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
                  message.Author.IsBot) return;

            // Create websocket command context based on the message
            var context = new SocketCommandContext(_client, message);

            // Execute the command
            await _commands.ExecuteAsync(context, argPos, _services);
        }

        private async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            // The command was successful, we don't care about this result, unless we want to log that a command succeeded.
            if (result.IsSuccess)
            {
                _logger.LogInformation($"Command [{command.Value.Name}] successfully executed for [{context.User.Username}] on [{context.Guild.Name}] in [{context.Channel.Name}]");
                return;
            }
            
            // The command failed, so we notify the user that something happened.
            _logger.LogError($"Command [{command.Value.Name}] unsuccessfully executed for [{context.User.Username}] on [{context.Guild.Name}] in [{context.Channel.Name}]");
            switch (result.Error)
            {
                case CommandError.Unsuccessful:
                    if (result is GameMasterResult gameMasterResult) await context.Channel.SendMessageAsync($"I couldn't process your command because {gameMasterResult.Reason}");
                    else await context.Channel.SendMessageAsync("I couldn't process your command. Try again, and if the error persists then log a bug with '!bug'.");
                    break;
                case CommandError.UnknownCommand:
                    // await context.Channel.SendMessageAsync("I don't know that command. Take a look at the command list using '!help'.");
                    break;
                case CommandError.UnmetPrecondition:
                    // TODO: Show the preconditions that were not met
                    // var errors = command.Value.Preconditions.Aggregate("You are not authorized to use this command. ", (current, precondition) => current + $"{precondition.ErrorMessage} ");
                    await context.Channel.SendMessageAsync("You are not authorized to use this command. Make sure you have the proper role and are in the appropriate channel before trying again.");
                    break;
                case CommandError.BadArgCount:
                    var expected = command.Value.Parameters.Count;
                    var got = context.Message.Content.Split(",(?=([^\"]*\"[^\"]*\")*[^\"]*$)").Length;
                    await context.Channel.SendMessageAsync($"You used the wrong number of arguments for that command. I expected {expected}, but I got {got}.");
                    break;
                case CommandError.ParseFailed:
                    await context.Channel.SendMessageAsync("I couldn't understand your command arguments. Make sure you don't have a number where text should be and vice versa. Make sure all dates are of the form: DD/MM/YYYY.");
                    break;
                default: 
                    await context.Channel.SendMessageAsync("I ran into an unexpected error. Try again, and if the error persists then log a bug with '!bug'.");
                    break;
            }
        }
    }
}
