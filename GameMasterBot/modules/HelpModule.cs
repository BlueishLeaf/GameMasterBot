using System;
using System.Threading.Tasks;
using Discord.Commands;
using GameMasterBot.Utils;
// ReSharper disable UnusedMember.Global


namespace GameMasterBot.modules
{
    [RequireContext(ContextType.Guild)]
    [Name("Help")]
    public class HelpModule: ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;

        public HelpModule(CommandService service) => _service = service;

        [Command("help"), Alias("?"), Summary("Displays a list of all the commands for GameMaster bot.")]
        public async Task<RuntimeResult> HelpAsync()
        {
            try
            {
                await ReplyAsync("Commands retrieved successfully!", false, EmbedUtils.ModulesEmbed(_service.Modules));
                return GameMasterResult.SuccessResult("Commands retrieved successfully.");
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult($"Command failed, Error: {e.Message}");
            }
        }

        [Command("help"), Alias("?"), Summary("Displays information about the specified command.")]
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

            try
            {
                await ReplyAsync("Commands found successfully!", false, EmbedUtils.CommandsEmbed(searchResult.Commands));
                return GameMasterResult.SuccessResult("Command found successfully.");
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult($"Command failed, Error: {e.Message}");
            }
        }
    }
}
