using System;
using System.Reflection;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using GameMasterBot.Extensions;

namespace GameMasterBot.Services
{
    public class InteractionHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _interactionService;
        private readonly IServiceProvider _services;

        public InteractionHandler(DiscordSocketClient client, InteractionService interactionService, IServiceProvider services)
        {
            _client = client;
            _interactionService = interactionService;
            _services = services;

            // Handle outcome of commands
            _interactionService.SlashCommandExecuted += SlashCommandExecuted;

            // Handle commands
            _client.InteractionCreated += HandleInteractionAsync;
        }

        public async Task InitializeAsync() => await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

        private async Task HandleInteractionAsync(SocketInteraction interaction)
        {
            // Create websocket execution context
            var context = new SocketInteractionContext(_client, interaction);

            try
            {
                // Execute the interaction
                await _interactionService.ExecuteCommandAsync(context, _services);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                if (interaction.Type == InteractionType.ApplicationCommand)
                {
                    // Delete original message if something goes wrong
                    await interaction.GetOriginalResponseAsync()
                        .ContinueWith(async (msg) => await msg.Result.DeleteAsync());
                }
            }
        }
        
        private static async Task SlashCommandExecuted(SlashCommandInfo slashCommandInfo, IInteractionContext interactionContext, IResult result)
        {
            if (!result.IsSuccess)
            {
                Console.WriteLine(result.Error);

                var errorMessage = "Sorry, I ran into an unexpected error while trying to execute your command, please try again. If the error persists then bug Killian!";
                if (result.Error == InteractionCommandError.Unsuccessful)
                {
                    if (result is CommandResult commandResult && !commandResult.ErrorReason.IsNullOrEmpty())
                    {
                        errorMessage = commandResult.ErrorReason;
                    }
                }

                // Modify the original response if one exists, otherwise respond as normal
                try
                {
                    await interactionContext.Interaction.RespondAsync(errorMessage, ephemeral: true);
                }
                catch (InvalidOperationException)
                {
                    await interactionContext.Interaction.ModifyOriginalResponseAsync(message =>
                    {
                        message.Content = errorMessage;
                    });
                }
            }
        }
    }
}
