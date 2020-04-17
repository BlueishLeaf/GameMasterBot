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

        [Command("version"), Alias("v")]
        [Summary("Displays this bot's version number.")]
        public async Task<RuntimeResult> VersionAsync()
        {
            await ReplyAsync("GameMasterBot v1.0.0");
            return GameMasterResult.SuccessResult();
        }

        [Command("roadmap"), Alias("repo", "repository", "issues")]
        [Summary("Displays a link to the repository of this bot.")]
        public async Task<RuntimeResult> RoadmapAsync()
        {
            await ReplyAsync("You can check the roadmap and source code at: https://github.com/BlueishLeaf/GameMasterBot");
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
