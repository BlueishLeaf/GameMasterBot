using System.Threading.Tasks;
using Discord.Commands;
using GameMasterBot.Utilities;
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace GameMasterBot.modules
{
    [RequireContext(ContextType.Guild)]
    [Name("Help")]
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;

        public HelpModule(CommandService service) => _service = service;

        [Command("help"), Alias("?")]
        [Summary("Displays a list of all the commands for this bot.")]
        public async Task<RuntimeResult> HelpAsync()
        {
            await ReplyAsync(embed: EmbedBuilder.ModuleList(_service.Modules));
            return GameMasterResult.SuccessResult();
        }

        [Command("help"), Alias("?")]
        [Summary("Displays information about a specified command.")]
        public async Task<RuntimeResult> HelpAsync(
            [Summary("The command to be searched for.")] string command)
        {
            #region Validation

            #region Command

            var searchResult = _service.Search(Context, command);
            if (!searchResult.IsSuccess)
                return GameMasterResult.ErrorResult($"Could not find any commands matching {command}");

            #endregion

            #endregion

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
        
        [Command("tutorial"), Alias("overview")]
        [Summary("Displays a tutorial on how to use the bot.")]
        public async Task<RuntimeResult> OverviewAsync()
        {
            await ReplyAsync(embed: EmbedBuilder.Overview());
            return GameMasterResult.SuccessResult();
        }
    }
}
