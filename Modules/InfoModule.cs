using System.Threading.Tasks;
using Discord.Interactions;
using GameMasterBot.Embeds;
using GameMasterBot.Extensions;

namespace GameMasterBot.Modules
{
    [RequireContext(ContextType.Guild)]
    public class InfoModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("roadmap", "Displays a link to the roadmap of planned features for this bot.")]
        public async Task<RuntimeResult> ShowRoadmapAsync()
        {
            await RespondAsync("You can check the roadmap and source code at: https://github.com/BlueishLeaf/GameMasterBot/projects/1", ephemeral: true);
            return CommandResult.AsSuccess();
        }
        
        [SlashCommand("repository", "Displays a link to the GitHub repository for this bot.")]
        public async Task<RuntimeResult> ShowRepositoryAsync()
        {
            await RespondAsync("You can check out the source code at: https://github.com/BlueishLeaf/GameMasterBot", ephemeral: true);
            return CommandResult.AsSuccess();
        }
        
        [SlashCommand("tutorial", "Displays a brief overview of how to use this bot.")]
        public async Task<RuntimeResult> ShowOverviewAsync()
        {
            await RespondAsync(embed: BotEmbeds.Tutorial(), ephemeral: true);
            return CommandResult.AsSuccess();
        }
    }
}
