using System.Threading.Tasks;
using Discord.Commands;
using GameMasterBot.Embeds;
using GameMasterBot.Extensions;

namespace GameMasterBot.Modules
{
    [RequireContext(ContextType.Guild)]
    [Name("Help")]
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;

        public HelpModule(CommandService service) => _service = service;

        [Command("help")]
        [Summary("Displays a list of all the commands for this bot.")]
        public async Task<RuntimeResult> HelpAsync()
        {
            await ReplyAsync(embed: EmbedBuilder.ModuleList(_service.Modules));
            return GameMasterResult.SuccessResult();
        }

        [Command("help")]
        [Summary("Displays information about a specified command.")]
        public async Task<RuntimeResult> HelpAsync(
            [Summary("The command to be searched for.")] string command)
        {
            var searchResult = _service.Search(Context, command);
            if (!searchResult.IsSuccess)
                return GameMasterResult.ErrorResult($"Could not find any commands matching {command}");

            await ReplyAsync(embed: EmbedBuilder.CommandList(searchResult.Commands));
            return GameMasterResult.SuccessResult();
        }

        [Command("project"), Alias("issues", "roadmap")]
        [Summary("Displays a link to the kanban board for this bot.")]
        public async Task<RuntimeResult> ProjectAsync()
        {
            await ReplyAsync("You can check the roadmap and source code at: https://github.com/BlueishLeaf/GameMasterBot/projects/1");
            return GameMasterResult.SuccessResult();
        }
        
        [Command("repository"), Alias("repo", "code")]
        [Summary("Displays a link to the GitHub repository for this bot.")]
        public async Task<RuntimeResult> RepositoryAsync()
        {
            await ReplyAsync("You can check out the source code at: https://github.com/BlueishLeaf/GameMasterBot");
            return GameMasterResult.SuccessResult();
        }
        
        [Command("tutorial")]
        [Summary("Displays a tutorial on how to use the bot.")]
        public async Task<RuntimeResult> OverviewAsync()
        {
            await ReplyAsync(embed: EmbedBuilder.Overview());
            return GameMasterResult.SuccessResult();
        }
    }
}
