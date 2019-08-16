using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Commands;
using GameMasterBot.Services;
using GameMasterBot.Utils;
// ReSharper disable UnusedMember.Global

namespace GameMasterBot.Modules
{
    [Group("campaign")]
    public class CampaignModule : ModuleBase<SocketCommandContext>
    {
        private readonly CampaignService _service;

        public CampaignModule(CampaignService service) => _service = service;

        [Command("add")]
        [Summary("Creates a new campaign on this server.")]
        public async Task<RuntimeResult> AddAsync(
            [Summary("The campaign's name.")] string name,
            [Summary("The campaign's system.")] string system,
            [Summary("The campaign's game master.")] string gameMaster,
            [Summary("The campaign's url.")] string url,
            [Summary("The campaign's players.")] params string[] players)
        {
            #region Validation

            var channelRegex = new Regex("^[a-zA-Z0-9 ]*$");

            #region Name

            // Max channel name length in Discord is 100 characters, check against 90 to be safe
            if (name.Length > 90)
            {
                return ResultBuilder.ErrorResult("The campaign's name must be less than 100 characters long.");
            }
            // Text channels cannot have special characters in their name
            if (!channelRegex.IsMatch(name))
            {
                return ResultBuilder.ErrorResult("The campaign's name must only contain alphanumeric characters and spaces.");
            }

            #endregion

            #region System

            // We will be creating a category channel from the system name later, so we check the length
            if (system.Length > 90)
            {
                return ResultBuilder.ErrorResult("The campaign's system must be less than 100 characters long.");
            }
            if (!channelRegex.IsMatch(name))
            {
                return ResultBuilder.ErrorResult("The campaign's system must only contain alphanumeric characters and spaces.");
            }

            #endregion

            #region GameMaster

            // Check if GM exists in the server
            if (Context.Guild.Users.FirstOrDefault(user => user.Username == gameMaster || user.Nickname == gameMaster) == null)
            {
                return ResultBuilder.ErrorResult($"The campaign's GM({gameMaster}) was not found in this server.");
            }

            #endregion

            #region Url

            // Check if valid url
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                return ResultBuilder.ErrorResult("The campaign's URL is not valid.");
            }

            #endregion

            #region Players

            // Check that each player is a member of the server
            foreach (var player in players)
            {
                if (Context.Guild.Users.FirstOrDefault(user => user.Username == player || user.Nickname == player) == null)
                {
                    return ResultBuilder.ErrorResult($"One of the campaign's players({player}) was not found in this server.");
                }
            }

            #endregion

            #endregion

            // Send input to service to create the campaign and create rooms
            var campaign = await _service.Create(name, system, gameMaster, url, players);

            // Return a rich text embed representing the new campaign
            return ResultBuilder.SuccessResult($"Campaign({campaign.Name}) Created Successfully!");
        }
    }
}
