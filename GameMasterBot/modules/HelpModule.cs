﻿using System;
using System.Threading.Tasks;
using Discord.Commands;
using GameMasterBot.Utils;
// ReSharper disable UnusedMember.Global


namespace GameMasterBot.modules
{
    [RequireContext(ContextType.Guild)]
    [Name("Help")]
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;

        public HelpModule(CommandService service) => _service = service;

        [Command("help"), Alias("?"), Name("help"), Summary("Displays a list of all the commands for this bot.")]
        public async Task<RuntimeResult> HelpAsync()
        {
            try
            {
                await ReplyAsync("Commands retrieved successfully!", false, EmbedUtils.ModulesEmbed(_service.Modules));
                return GameMasterResult.SuccessResult();
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult($"Command failed, Error: {e.Message}");
            }
        }

        [Command("help"), Alias("?"), Name("help"), Summary("Displays information about a specified command.")]
        public async Task<RuntimeResult> HelpAsync(
            [Summary("The command to be searched for.")]
            string command)
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
                await ReplyAsync("Commands found successfully!", false,
                    EmbedUtils.CommandsEmbed(searchResult.Commands));
                return GameMasterResult.SuccessResult();
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult($"Command failed, Error: {e.Message}");
            }
        }

        [Command("version"), Alias("v"), Name("version"), Summary("Displays this bot's version number.")]
        public async Task<RuntimeResult> VersionAsync()
        {
            await ReplyAsync("GameMasterBot Alpha v0.1.1");
            return GameMasterResult.SuccessResult();
        }

        [Command("roadmap"), Alias("repo", "repository", "issues"), Name("roadmap"), Summary("Displays a link to the repository of this bot.")]
        public async Task<RuntimeResult> RoadmapAsync()
        {
            await ReplyAsync("You can check the roadmap and source code at https://github.com/BlueishLeaf/GameMasterBot");
            return GameMasterResult.SuccessResult();
        }
    }
}
