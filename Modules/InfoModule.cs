using System.Threading.Tasks;
using Discord.Interactions;
using GameMasterBot.Extensions;
using GameMasterBot.Utils;
// Modules and their methods are picked up by the handler but not recognised by Rider
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace GameMasterBot.Modules
{
    [RequireContext(ContextType.Guild)]
    public class InfoModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("support", "Displays a link to the GitHub repository for this bot.")]
        public async Task<RuntimeResult> ShowRepositoryAsync()
        {
            await RespondAsync("You can check out the GitHub repo and leave a bug ticket at: https://github.com/BlueishLeaf/GameMasterBot", ephemeral: true);
            return CommandResult.AsSuccess();
        }
        
        [SlashCommand("tutorial", "Displays a brief overview of how to use this bot.")]
        public async Task<RuntimeResult> ShowOverviewAsync()
        {
            await RespondAsync(embed: InfoEmbedBuilder.BuildTutorialEmbed(), ephemeral: true);
            return CommandResult.AsSuccess();
        }
    }
}
