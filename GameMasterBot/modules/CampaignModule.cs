using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
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

        [Command("add", RunMode = RunMode.Async), Alias("+"), Summary("Creates a new campaign on this server.")]
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
            if (Context.Guild.Users.FirstOrDefault(user => user.Username == gameMaster || user.Nickname == gameMaster) == null)
                return GameMasterResult.ErrorResult($"The campaign's GM({gameMaster}) was not found in this server.");

            #endregion

            #region Url

            // Check if valid url
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                return GameMasterResult.ErrorResult("The campaign's URL is not valid.");

            #endregion

            #region Players

            foreach (var player in players)
                if (Context.Guild.Users.FirstOrDefault(user => string.Equals(user.Username, player, StringComparison.CurrentCultureIgnoreCase) || string.Equals(user.Nickname, player, StringComparison.CurrentCultureIgnoreCase)) == null)
                    return GameMasterResult.ErrorResult($"One of the campaign's players({player}) was not found in this server.");

            #endregion

            #endregion

            // Send input to service to create the campaign in the database
            var campaign = _service.Create(name, system, gameMaster, url, players, Context.User.ToString(), Context.Guild.Name, Context.Guild.Id);

            #region Guild Administration

            var campaignRole = Context.Guild.Roles.FirstOrDefault(role => role.Name == $"{campaign.Name} Player") ??
                (IRole)Context.Guild.CreateRoleAsync($"{campaign.Name} Player").Result;

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
            await campaignTextChannel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole,
                new OverwritePermissions(viewChannel: PermValue.Deny));
            await campaignTextChannel.AddPermissionOverwriteAsync(campaignRole,
                new OverwritePermissions(viewChannel: PermValue.Allow));

            // Create the voice channel for this campaign if one does not exist
            var campaignVoiceChannel = Context.Guild.VoiceChannels.FirstOrDefault(chan => chan.Name == campaign.Name) ??
                                       (IVoiceChannel)Context.Guild.CreateVoiceChannelAsync(campaign.Name,
                                           channel => channel.CategoryId = campaignCategoryChannel.Id).Result;

            // Set the permissions on the campaign's voice channel
            await campaignVoiceChannel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole,
                new OverwritePermissions(viewChannel: PermValue.Deny));
            await campaignVoiceChannel.AddPermissionOverwriteAsync(campaignRole,
                new OverwritePermissions(viewChannel: PermValue.Allow));

            // Add the campaign role to each of the players
            foreach (var player in campaign.Players)
                await Context.Guild.Users.First(user =>
                        string.Equals(user.Username, player, StringComparison.CurrentCultureIgnoreCase) ||
                        string.Equals(user.Nickname, player, StringComparison.CurrentCultureIgnoreCase))
                    .AddRoleAsync(campaignRole);

            #endregion

            // Send a rich text embed representing the new campaign
            await ReplyAsync("Campaign created successfully!", false, EmbedUtils.CampaignEmbed(campaign));

            return GameMasterResult.SuccessResult($"Campaign({campaign.Name}) created successfully.");
        }

        [Command("remove", RunMode = RunMode.Async), Alias("-"), Summary("Removes a campaign from this server.")]
        public async Task<RuntimeResult> RemoveAsync(
            [Summary("The Campaign's name.")] string name)
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
            var textChannel = Context.Guild.TextChannels.FirstOrDefault(channel => channel.Name == campaignId);
            if (textChannel != null) await Context.Guild.GetTextChannel(textChannel.Id).DeleteAsync();

            // Delete the voice channel for this campaign if it exists
            var voiceChannel = Context.Guild.VoiceChannels.FirstOrDefault(channel => channel.Name.ToLower().Replace(' ', '-') == campaignId);
            if (voiceChannel != null) await Context.Guild.GetVoiceChannel(voiceChannel.Id).DeleteAsync();

            // Delete the role for this campaign if it exists
            var campaignRole = Context.Guild.Roles.FirstOrDefault(role => role.Name.ToLower().Replace(' ', '-') == $"{campaignId}-player");
            if (campaignRole != null) await Context.Guild.GetRole(campaignRole.Id).DeleteAsync();

            #endregion

            try
            {
                _service.Remove(Context.Guild.Id.ToString(), campaignId);
                await ReplyAsync($"Campaign removed successfully!");
                return GameMasterResult.SuccessResult($"Campaign({name}) removed successfully.");
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult($"Command failed, Error: {e.Message}");
            }
        }

        [Command("info"), Summary("Returns information about the campaign belonging to this channel.")]
        public async Task<RuntimeResult> GetAsync()
        {
            try
            {
                var campaign = await _service.Get(Context.Guild.Id.ToString(), Context.Channel.Name);
                await ReplyAsync("Campaign found successfully!", false, EmbedUtils.CampaignEmbed(campaign));
                return GameMasterResult.SuccessResult($"Campaign({campaign.Name}) found successfully.");
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult($"Command failed, Error: {e.Message}");
            }
        }

        [Command("info"), Summary("Returns information about the campaign specified.")]
        public async Task<RuntimeResult> GetAsync(
            [Summary("The Campaign's name.")] string name)
        {
            #region Validation

            #region Name

            // Max channel name length in Discord is 100 characters, check against 90 to be safe
            if (name.Length > 90)
                return GameMasterResult.ErrorResult("The campaign's name must be less than 100 characters long.");

            #endregion

            #endregion

            try
            {
                var campaign = await _service.Get(Context.Guild.Id.ToString(), name.ToLower().Replace(' ', '-'));
                await ReplyAsync("Campaign found successfully!", false, EmbedUtils.CampaignEmbed(campaign));
                return GameMasterResult.SuccessResult($"Campaign({name}) found successfully.");
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult($"Command failed, Error: {e.Message}");
            }
        }

        [Command("server"), Alias("*"), Summary("Returns information about all campaigns on this server.")]
        public async Task<RuntimeResult> GetAllAsync()
        {
            try
            {
                var campaigns = _service.GetForServer(Context.Guild.Id.ToString());
                await ReplyAsync("Campaigns found successfully! For more information about a campaign, type '!campaign info [Campaign]'", false, EmbedUtils.CampaignsEmbed(campaigns));
                return GameMasterResult.SuccessResult($"Campaigns({campaigns.Count()}) found successfully.");
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult($"Command failed, Error: {e.Message}");
            }
        }

        [Command("self"), Alias("me"), Summary("Returns information about all campaigns for this player.")]
        public async Task<RuntimeResult> GetForPlayerAsync()
        {
            var guildUser = Context.Guild.Users.FirstOrDefault(user => string.Equals(user.Username, Context.User.Username, StringComparison.CurrentCultureIgnoreCase));
            if (guildUser == null)
                return GameMasterResult.ErrorResult("The user specified does not exist in this server");
            try
            {
                var campaigns = _service.GetForPlayer(Context.Guild.Id.ToString(), guildUser.Id.ToString());
                await guildUser.SendMessageAsync("Campaigns found successfully! For more information about a campaign, type '!campaign info [Campaign]'", false, EmbedUtils.CampaignsEmbed(campaigns));
                return GameMasterResult.SuccessResult($"Campaigns({campaigns.Count()}) found successfully.");
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult($"Command failed, Error: {e.Message}");
            }
        }
    }
}
