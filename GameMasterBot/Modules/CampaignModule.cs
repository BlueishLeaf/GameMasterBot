using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using GameMasterBot.Services;
using GameMasterBot.Utilities;
using EmbedBuilder = GameMasterBot.Utilities.EmbedBuilder;
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace GameMasterBot.Modules
{
    [RequireContext(ContextType.Guild)]
    [Group("campaign"), Name("Campaign")]
    public class CampaignModule : ModuleBase<SocketCommandContext>
    {
        private readonly CampaignService _campaignService;
        private readonly SessionService _sessionService;

        public CampaignModule(CampaignService campaignService, SessionService sessionService)
        {
            _campaignService = campaignService;
            _sessionService = sessionService;
        }

        [RequireRole("Whitelisted")]
        [Command("add", RunMode = RunMode.Async), Alias("create", "+")]
        [Summary("Creates a new campaign on this server, including channels and roles.")]
        public async Task<RuntimeResult> AddAsync(
            [Summary("The name of the campaign.")] string name,
            [Summary("The game system the campaign will use.")] string system,
            [Summary("The username of the game master.")] string gameMaster,
            [Summary("The Roll20 url for the campaign.")] string url,
            [Summary("The campaign's players, separated by a space.")] params string[] players)
        {
            #region Validation

            var channelRegex = new Regex("^[a-zA-Z0-9 ]*$");

            #region Name

            // Max channel name length in Discord is 100 characters, check against 90 to be safe
            if (name.Length > 90)
                return GameMasterResult.ErrorResult("The campaign's name must be less than 100 characters long.");
            // Text channels cannot have special characters in their name
            if (!channelRegex.IsMatch(name))
                return GameMasterResult.ErrorResult("The campaign's name must only contain alphanumeric characters and spaces.");

            #endregion

            #region System

            // We will be creating a category channel from the system name later, so we check the length
            if (system.Length > 90)
                return GameMasterResult.ErrorResult("The campaign's system must be less than 100 characters long.");
            if (!channelRegex.IsMatch(name))
                return GameMasterResult.ErrorResult("The campaign's system must only contain alphanumeric characters and spaces.");

            #endregion

            #region GameMaster

            // Check if GM exists in the server
            var gmUser = Context.Guild.Users.FirstOrDefault(user => user.Username == gameMaster || user.Nickname == gameMaster);
            if (gmUser == null)
                return GameMasterResult.ErrorResult($"The campaign's GM({gameMaster}) was not found in this server.");

            #endregion

            #region Url

            // Check if valid url
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                return GameMasterResult.ErrorResult("The campaign's URL is not valid.");

            #endregion

            #region Players

            var playerUsernames = new List<string>();
            foreach (var player in players)
            {
                var guildPlayer = Context.Guild.Users.FirstOrDefault(user =>
                    string.Equals(user.Username, player, StringComparison.CurrentCultureIgnoreCase) ||
                    string.Equals(user.Nickname, player, StringComparison.CurrentCultureIgnoreCase));
                if (guildPlayer == null)
                    return GameMasterResult.ErrorResult($"One of the campaign's players({player}) was not found in this server.");
                playerUsernames.Add(guildPlayer.Username);
            }
            
            #endregion

            #endregion

            try
            {
                // Send input to service to create the campaign in the database
                var campaign = _campaignService.Create(name, system, gmUser.Username, gmUser.Id, url, playerUsernames, Context.User.ToString(), Context.Guild.Name, Context.Guild.Id);

                #region Guild Administration

                // TODO Move elsewhere, and allow users to pick color
                Color roleColor;
                var random = new Random();
                roleColor = random.Next(5) switch
                {
                    0 => Color.Blue,
                    1 => Color.Green,
                    2 => Color.Purple,
                    3 => Color.Orange,
                    4 => Color.Red,
                    5 => Color.Teal,
                    _ => Color.Default
                };

                var playerRole = Context.Guild.Roles.FirstOrDefault(role => role.Name == $"Player: {campaign.Name}") ??
                                 (IRole)Context.Guild.CreateRoleAsync($"Player: {campaign.Name}", null, roleColor).Result;

                var gmRole = Context.Guild.Roles.FirstOrDefault(role => role.Name == $"Game Master: {campaign.Name}") ??
                                 (IRole)Context.Guild.CreateRoleAsync($"Game Master: {campaign.Name}", null, roleColor).Result;

                // Create the category channel for this campaign's system if one does not already exist
                var campaignCategoryChannel = Context.Guild.CategoryChannels.FirstOrDefault(cat => cat.Name == campaign.System) ??
                                                  (ICategoryChannel)Context.Guild.CreateCategoryChannelAsync(campaign.System).Result;

                // Create the text channel for this campaign if one does not exist
                var campaignTextChannel = Context.Guild.TextChannels.FirstOrDefault(chan => chan.Name == campaign.Id) ??
                                          (ITextChannel)Context.Guild.CreateTextChannelAsync(campaign.Id, channel =>
                                          {
                                              channel.CategoryId = campaignCategoryChannel.Id;
                                              channel.Topic = $"Channel for discussing the {campaign.System} campaign {campaign.Name}";
                                          }).Result;

                // Set the permissions on the campaign's text channel
                await campaignTextChannel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, new OverwritePermissions(readMessageHistory: PermValue.Deny, sendMessages: PermValue.Deny, viewChannel: PermValue.Deny));
                await campaignTextChannel.AddPermissionOverwriteAsync(playerRole, new OverwritePermissions(sendMessages: PermValue.Allow, readMessageHistory: PermValue.Allow, viewChannel: PermValue.Allow, attachFiles: PermValue.Allow));
                await campaignTextChannel.AddPermissionOverwriteAsync(gmRole, new OverwritePermissions(sendMessages: PermValue.Allow, readMessageHistory: PermValue.Allow, manageMessages: PermValue.Allow, manageChannel: PermValue.Allow, viewChannel: PermValue.Allow, attachFiles: PermValue.Allow));

                // Create the voice channel for this campaign if one does not exist
                var campaignVoiceChannel = Context.Guild.VoiceChannels.FirstOrDefault(chan => chan.Name == campaign.Name) ??
                                           (IVoiceChannel)Context.Guild.CreateVoiceChannelAsync(campaign.Name,
                                               channel => channel.CategoryId = campaignCategoryChannel.Id).Result;

                // Set the permissions on the campaign's voice channel
                await campaignVoiceChannel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, new OverwritePermissions(connect: PermValue.Deny, viewChannel: PermValue.Deny));
                await campaignVoiceChannel.AddPermissionOverwriteAsync(playerRole, new OverwritePermissions(connect: PermValue.Allow, viewChannel: PermValue.Allow, speak: PermValue.Allow, useVoiceActivation: PermValue.Allow));
                await campaignVoiceChannel.AddPermissionOverwriteAsync(gmRole, new OverwritePermissions(connect: PermValue.Allow, manageChannel: PermValue.Allow, viewChannel: PermValue.Allow, speak: PermValue.Allow, useVoiceActivation: PermValue.Allow));

                // Add the gm role to the game master
                await Context.Guild.Users.First(user => user.Username == gameMaster || user.Nickname == gameMaster).AddRoleAsync(gmRole);

                // Add the campaign role to each of the players
                foreach (var player in campaign.Players)
                    await Context.Guild.Users.First(user =>
                            string.Equals(user.Username, player, StringComparison.CurrentCultureIgnoreCase) ||
                            string.Equals(user.Nickname, player, StringComparison.CurrentCultureIgnoreCase))
                        .AddRoleAsync(playerRole);

                #endregion

                // Send a rich text embed representing the new campaign
                await ReplyAsync("Campaign created successfully!", embed: EmbedBuilder.CampaignInfo(campaign));
                return GameMasterResult.SuccessResult();
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult(e.Message);
            }
        }

        [Command("remove", RunMode = RunMode.Async), Alias("delete", "-")]
        [Summary("Removes a campaign from this server, including channels and roles.")]
        public async Task<RuntimeResult> RemoveAsync(
            [Summary("The name of the campaign.")] string name)
        {
            #region Validation

            #region Name

            // Max channel name length in Discord is 100 characters, check against 90 to be safe
            if (name.Length > 90)
                return GameMasterResult.ErrorResult("The campaign's name must be less than 100 characters long.");

            #endregion

            #endregion

            #region Guild Administration

            var campaignId = name.ToLower().Replace(' ', '-');
            // Check to make sure that this user is the game master of the campaign
            var campaignInfo = await _campaignService.Get(Context.Guild.Id, campaignId);
            var commandIssuer = Context.Guild.GetUser(Context.User.Id);
            if (campaignInfo.GameMasterId != Context.User.Id && !commandIssuer.GuildPermissions.Administrator)                
                return GameMasterResult.ErrorResult("You do not have permission to remove this campaign.");
            
            var textChannel = Context.Guild.TextChannels.FirstOrDefault(channel => channel.Name == campaignId);
            if (textChannel != null) await Context.Guild.GetTextChannel(textChannel.Id).DeleteAsync();

            // Delete the voice channel for this campaign if it exists
            var voiceChannel = Context.Guild.VoiceChannels.FirstOrDefault(channel => channel.Name.ToLower().Replace(' ', '-') == campaignId);
            if (voiceChannel != null) await Context.Guild.GetVoiceChannel(voiceChannel.Id).DeleteAsync();

            // Delete the roles for this campaign if they exists
            var campaignRole = Context.Guild.Roles.FirstOrDefault(role => role.Name.ToLower().Replace(' ', '-') == $"player:-{campaignId}");
            if (campaignRole != null) await Context.Guild.GetRole(campaignRole.Id).DeleteAsync();
            var gmRole = Context.Guild.Roles.FirstOrDefault(role => role.Name.ToLower().Replace(' ', '-') == $"game-master:-{campaignId}");
            if (gmRole != null) await Context.Guild.GetRole(gmRole.Id).DeleteAsync();

            #endregion

            try
            {
                _campaignService.Remove(Context.Guild.Id, campaignId);
                await ReplyAsync("Campaign removed successfully!");
                return GameMasterResult.SuccessResult();
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult(e.Message);
            }
        }

        [Command("info"), Alias("details")]
        [Summary("Returns all information about the campaign specified.")]
        public async Task<RuntimeResult> InfoAsync(
            [Summary("The name of the campaign.")] string campaign = "")
        {
            #region Validation

            #region Campaign

            string campaignId;
            if (string.IsNullOrEmpty(campaign))
            {
                campaignId = Context.Channel.Name;
                var campaignTextChannel = Context.Guild.TextChannels.FirstOrDefault(chan => chan.Name == campaignId);
                if (campaignTextChannel == null)
                    return GameMasterResult.ErrorResult("Campaign does not exist on this server.");
            }
            else
            {
                campaignId = campaign.Replace(' ', '-').ToLower();
                var campaignTextChannel = Context.Guild.TextChannels.FirstOrDefault(chan => chan.Name == campaignId);
                if (campaignTextChannel == null)
                    return GameMasterResult.ErrorResult("Campaign does not exist on this server.");
            }

            #endregion

            #endregion

            try
            {
                var campaignInfo = await _campaignService.Get(Context.Guild.Id, campaignId);
                var sessionInfo = _sessionService.GetUpcoming(Context.Guild.Id, campaignId);
                await ReplyAsync(embed: EmbedBuilder.CampaignSummary(campaignInfo, sessionInfo));
                return GameMasterResult.SuccessResult();
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult(e.Message);
            }
        }
        
        [Command("add-player"), Alias("ap")]
        [Summary("Adds a player to an existing campaign.")]
        public async Task<RuntimeResult> AddPlayerAsync(
            [Summary("The username of the player.")] string player,
            [Summary("The name of the campaign.")] string campaign = "")
        {
            #region Validation
            
            #region Player

            var guildUser = Context.Guild.Users.FirstOrDefault(user =>
                string.Equals(user.Username, player, StringComparison.CurrentCultureIgnoreCase) ||
                string.Equals(user.Nickname, player, StringComparison.CurrentCultureIgnoreCase));
            if (guildUser == null)
                return GameMasterResult.ErrorResult("That player does not exist in this server.");
            
            #endregion

            #region Campaign

            string campaignId;
            if (string.IsNullOrEmpty(campaign))
            {
                campaignId = Context.Channel.Name;
                var campaignTextChannel = Context.Guild.TextChannels.FirstOrDefault(chan => chan.Name == campaignId);
                if (campaignTextChannel == null)
                    return GameMasterResult.ErrorResult("Campaign does not exist on this server.");
            }
            else
            {
                campaignId = campaign.Replace(' ', '-').ToLower();
                var campaignTextChannel = Context.Guild.TextChannels.FirstOrDefault(chan => chan.Name == campaignId);
                if (campaignTextChannel == null)
                    return GameMasterResult.ErrorResult("Campaign does not exist on this server.");
            }

            #endregion

            #endregion

            try
            {
                var campaignInfo = await _campaignService.Get(Context.Guild.Id, campaignId);
                var commandIssuer = Context.Guild.GetUser(Context.User.Id);
                if (campaignInfo.GameMasterId != Context.User.Id && !commandIssuer.GuildPermissions.Administrator)
                    return GameMasterResult.ErrorResult("You do not have permission to remove this campaign.");
                // If there are no players, instantiate the list
                if (campaignInfo.Players == null)
                    campaignInfo.Players = new List<string>();
                // Check if player is already in the campaign
                if (campaignInfo.Players.Contains(guildUser.Username))
                    return GameMasterResult.ErrorResult("Player is already a member of this campaign.");
                campaignInfo.Players.Add(guildUser.Username);
                // Campaigns are immutable, so we need to delete and add it again with the new player
                _campaignService.Remove(Context.Guild.Id, campaignId);
                var newCampaignInfo = _campaignService.Create(campaignInfo);
                // Give the new player the campaign role
                var campaignRole = Context.Guild.Roles.FirstOrDefault(role => role.Name.ToLower().Replace(' ', '-') == $"player:-{campaignId}");
                if (campaignRole == null)
                    return GameMasterResult.ErrorResult("No player role exists for this campaign.");
                await guildUser.AddRoleAsync(campaignRole);
                await ReplyAsync($"Successfully added {guildUser.Username} as a player.", embed: EmbedBuilder.CampaignInfo(newCampaignInfo));
                return GameMasterResult.SuccessResult();
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult(e.Message);
            }
        }
        
        [Command("remove-player"), Alias("rp")]
        [Summary("Removes a player from an existing campaign.")]
        public async Task<RuntimeResult> RemovePlayerAsync(
            [Summary("The username of the player.")] string player,
            [Summary("The name of the campaign.")] string campaign = "")
        {
            #region Validation
            
            #region Player

            var guildUser = Context.Guild.Users.FirstOrDefault(user =>
                string.Equals(user.Username, player, StringComparison.CurrentCultureIgnoreCase) ||
                string.Equals(user.Nickname, player, StringComparison.CurrentCultureIgnoreCase));
            if (guildUser == null)
                return GameMasterResult.ErrorResult("That player does not exist in this server.");
            
            #endregion

            #region Campaign

            string campaignId;
            if (string.IsNullOrEmpty(campaign))
            {
                campaignId = Context.Channel.Name;
                var campaignTextChannel = Context.Guild.TextChannels.FirstOrDefault(chan => chan.Name == campaignId);
                if (campaignTextChannel == null)
                    return GameMasterResult.ErrorResult("Campaign does not exist on this server.");
            }
            else
            {
                campaignId = campaign.Replace(' ', '-').ToLower();
                var campaignTextChannel = Context.Guild.TextChannels.FirstOrDefault(chan => chan.Name == campaignId);
                if (campaignTextChannel == null)
                    return GameMasterResult.ErrorResult("Campaign does not exist on this server.");
            }

            #endregion

            #endregion

            try
            {
                var campaignInfo = await _campaignService.Get(Context.Guild.Id, campaignId);
                var commandIssuer = Context.Guild.GetUser(Context.User.Id);
                if (campaignInfo.GameMasterId != Context.User.Id && !commandIssuer.GuildPermissions.Administrator)
                    return GameMasterResult.ErrorResult("You do not have permission to remove a player from this campaign.");
                // Check if player is actually not in the campaign
                if (!campaignInfo.Players.Contains(guildUser.Username))
                    return GameMasterResult.ErrorResult("Player is not a member of this campaign.");
                campaignInfo.Players.Remove(guildUser.Username);
                // Campaigns are immutable, so we need to delete and add it again with the updated player list
                _campaignService.Remove(Context.Guild.Id, campaignId);
                var newCampaignInfo = _campaignService.Create(campaignInfo);
                // Remove the campaign role from the player
                var campaignRole = Context.Guild.Roles.FirstOrDefault(role => role.Name.ToLower().Replace(' ', '-') == $"player:-{campaignId}");
                if (campaignRole == null)
                    return GameMasterResult.ErrorResult("No player role exists for this campaign.");
                await guildUser.RemoveRoleAsync(campaignRole);
                await ReplyAsync($"Successfully removed {guildUser.Username} from the campaign.", embed: EmbedBuilder.CampaignInfo(newCampaignInfo));
                return GameMasterResult.SuccessResult();
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult(e.Message);
            }
        }

        [Command("server"), Alias("guild", "*")]
        [Summary("Returns all information about the campaigns on this server.")]
        public async Task<RuntimeResult> ServerAsync()
        {
            try
            {
                var campaigns = _campaignService.GetForServer(Context.Guild.Id);
                foreach (var campaign in campaigns)
                {
                    var sessions = _sessionService.GetUpcoming(Context.Guild.Id, campaign.Id);
                    await ReplyAsync(embed: EmbedBuilder.CampaignSummary(campaign, sessions));
                }
                return GameMasterResult.SuccessResult();
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult(e.Message);
            }
        }

        [Command("self"), Alias("me")]
        [Summary("Returns all information about the campaigns you are a part of.")]
        public async Task<RuntimeResult> SelfAsync()
        {
            var guildUser = Context.Guild.Users.FirstOrDefault(user => string.Equals(user.Username, Context.User.Username, StringComparison.CurrentCultureIgnoreCase));
            if (guildUser == null)
                return GameMasterResult.ErrorResult("The user specified does not exist in this server");
            try
            {
                var campaigns = _campaignService.GetForPlayer(Context.Guild.Id, guildUser.Username, guildUser.Nickname);
                foreach (var campaign in campaigns)
                {
                    var sessions = _sessionService.GetUpcoming(Context.Guild.Id, campaign.Id);
                    await guildUser.SendMessageAsync("I PMed you with your personal campaign details.", embed: EmbedBuilder.CampaignSummary(campaign, sessions));
                }
                return GameMasterResult.SuccessResult();
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult(e.Message);
            }
        }
    }
}
