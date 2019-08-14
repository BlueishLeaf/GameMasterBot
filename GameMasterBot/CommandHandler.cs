using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace GameMasterBot
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;

        // Retrieve client and CommandService instance via ctor
        public CommandHandler(DiscordSocketClient client, CommandService commands, IServiceProvider services)
        {
            _client = client;
            _commands = commands;
            _services = services;

            // Handle outcome of commands
            _commands.CommandExecuted += CommandExecutedAsync;

            // Handle commands
            _client.MessageReceived += HandleCommandAsync;
        }

        public async Task InitializeAsync() => await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

        public async Task HandleCommandAsync(SocketMessage messageParam)
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
        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            // The command is unspecified when there was a search failure (command not found); we don't care about these errors
            if (!command.IsSpecified) return;

            // The command was successful, we don't care about this result, unless we want to log that a command succeeded.
            if (result.IsSuccess) return;

            // The command failed, so we notify the user that something happened.
            await context.Channel.SendMessageAsync($"Error: {result}");
        }
    }
}
