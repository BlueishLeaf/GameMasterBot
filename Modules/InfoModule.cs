using System.Threading.Tasks;
using Discord.Interactions;
using GameMasterBot.Constants;
using GameMasterBot.Extensions;
using GameMasterBot.Messages;
using GameMasterBot.Utils;
// Modules and their methods are picked up by the handler but not recognised by Rider
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace GameMasterBot.Modules
{
    [RequireContext(ContextType.Guild)]
    public class InfoModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand(InfoCommands.SupportCommandName, InfoCommands.SupportCommandDescription)]
        public async Task<RuntimeResult> ShowRepositoryAsync()
        {
            await RespondAsync(InfoResponseMessages.LinkToRepository(), ephemeral: true);
            return CommandResult.AsSuccess();
        }
        
        [SlashCommand(InfoCommands.TutorialCommandName, InfoCommands.TutorialCommandDescription)]
        public async Task<RuntimeResult> ShowOverviewAsync()
        {
            await RespondAsync(embed: InfoEmbedBuilder.BuildTutorialEmbed(), ephemeral: true);
            return CommandResult.AsSuccess();
        }
    }
}
