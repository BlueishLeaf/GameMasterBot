﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using GameMasterBot.Extensions;
using GameMasterBot.Services;
using EmbedBuilder = GameMasterBot.Embeds.EmbedBuilder;

namespace GameMasterBot.Modules
{
    [RequireContext(ContextType.Guild)]
    [Group("campaign"), Name("Campaign")]
    public class CampaignModule : ModuleBase<SocketCommandContext>
    {
        private readonly CampaignService _campaignService;
        
        public CampaignModule(CampaignService campaignService) => _campaignService = campaignService;

        [RequireRole("Whitelisted")]
        [Command("add", RunMode = RunMode.Async), Alias("create")]
        [Summary("Creates a new campaign on this server, including channels and roles.")]
        public async Task<RuntimeResult> AddAsync(
            [Summary("The name of the campaign (Enclose in quotes if it has spaces).")] string name,
            [Summary("The game system the campaign will use (Enclose in quotes if it has spaces).")] string system)
        {
            var channelRegex = new Regex("^[a-zA-Z0-9 ]*$");
            const int channelMaxChars = 100;
            
            if (name.Length > channelMaxChars)
                return GameMasterResult.ErrorResult("the name of the campaign must be between 1 and 100 characters.");
            
            if (!channelRegex.IsMatch(name))
                return GameMasterResult.ErrorResult("the name of the campaign must only contain alphanumeric characters and spaces.");
            
            if (system.Length > channelMaxChars)
                return GameMasterResult.ErrorResult("the name of the campaign's system must be less than 100 characters long.");
            if (!channelRegex.IsMatch(name))
                return GameMasterResult.ErrorResult("the name of the campaign's system must only contain alphanumeric characters and spaces.");

            try
            {
                var message = await ReplyAsync("Creating campaign channels and roles...");
                
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
        
                var playerRole = Context.Guild.Roles.FirstOrDefault(role => role.Name == $"Player: {name}") ??
                                 (IRole)Context.Guild.CreateRoleAsync($"Player: {name}", null, roleColor).Result;
        
                var gmRole = Context.Guild.Roles.FirstOrDefault(role => role.Name == $"Game Master: {name}") ??
                                 (IRole)Context.Guild.CreateRoleAsync($"Game Master: {name}", null, roleColor).Result;
        
                // Create the category channel for this campaign's system if one does not already exist
                var campaignCategoryChannel = Context.Guild.CategoryChannels.FirstOrDefault(cat => cat.Name == system) ??
                                                  (ICategoryChannel)Context.Guild.CreateCategoryChannelAsync(system).Result;
        
                // Create the text channel for this campaign if one does not exist
                var textChannelName = name.ToLower().Replace(' ', '-');
                var campaignTextChannel = Context.Guild.TextChannels.FirstOrDefault(chan => chan.Name == textChannelName) ??
                                          (ITextChannel)Context.Guild.CreateTextChannelAsync(textChannelName, channel =>
                                          {
                                              channel.CategoryId = campaignCategoryChannel.Id;
                                              channel.Topic = $"Channel for discussing the {system} campaign {name}.";
                                          }).Result;
        
                // Set the permissions on the campaign's text channel
                await campaignTextChannel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, new OverwritePermissions(readMessageHistory: PermValue.Deny, sendMessages: PermValue.Deny, viewChannel: PermValue.Deny));
                await campaignTextChannel.AddPermissionOverwriteAsync(playerRole, new OverwritePermissions(sendMessages: PermValue.Allow, readMessageHistory: PermValue.Allow, viewChannel: PermValue.Allow, attachFiles: PermValue.Allow, addReactions: PermValue.Allow));
                await campaignTextChannel.AddPermissionOverwriteAsync(gmRole, new OverwritePermissions(sendMessages: PermValue.Allow, readMessageHistory: PermValue.Allow, manageMessages: PermValue.Allow, manageChannel: PermValue.Allow, viewChannel: PermValue.Allow, attachFiles: PermValue.Allow));
        
                // Create the voice channel for this campaign if one does not exist
                var campaignVoiceChannel = Context.Guild.VoiceChannels.FirstOrDefault(chan => chan.Name == name) ??
                                           (IVoiceChannel)Context.Guild.CreateVoiceChannelAsync(name,
                                               channel => channel.CategoryId = campaignCategoryChannel.Id).Result;
        
                // Set the permissions on the campaign's voice channel
                await campaignVoiceChannel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, new OverwritePermissions(connect: PermValue.Deny, viewChannel: PermValue.Deny));
                await campaignVoiceChannel.AddPermissionOverwriteAsync(playerRole, new OverwritePermissions(connect: PermValue.Allow, viewChannel: PermValue.Allow, speak: PermValue.Allow, useVoiceActivation: PermValue.Allow));
                await campaignVoiceChannel.AddPermissionOverwriteAsync(gmRole, new OverwritePermissions(connect: PermValue.Allow, manageChannel: PermValue.Allow, viewChannel: PermValue.Allow, speak: PermValue.Allow, useVoiceActivation: PermValue.Allow));
        
                // Add the gm role to the game master
                await Context.Guild.Users.First(user => user.Id == Context.User.Id).AddRoleAsync(gmRole);
                
                var campaign = await _campaignService.Create(name, system, Context.User, Context.Guild, campaignTextChannel.Id, campaignVoiceChannel.Id, playerRole.Id, gmRole.Id);

                await message.ModifyAsync(m =>
                {
                    m.Content = "Campaign created successfully!";
                    m.Embed = EmbedBuilder.CampaignInfo(campaign);
                });
                return GameMasterResult.SuccessResult();
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult(e.Message);
            }
        }
        
        [RequireRole("Whitelisted")]
        [Command("add-players")]
        [Summary("Adds one or more players to this campaign.")]
        public async Task<RuntimeResult> AddPlayersAsync(
            [Summary("The usernames/nicknames of the players in this server that you want to add.")] params string[] names)
        {
            var guildUsers = new List<SocketGuildUser>();
            foreach (var name in names)
            {
                var guildUser = Context.Guild.Users.FirstOrDefault(user =>
                    string.Equals(user.Username, name, StringComparison.CurrentCultureIgnoreCase) ||
                    string.Equals(user.Nickname, name, StringComparison.CurrentCultureIgnoreCase));
                if (guildUser == null)
                    await ReplyAsync($"I couldn't find a user by the name of '{name}' in this server, so I'll skip them.");
                else
                    guildUsers.Add(guildUser);
            }
            try
            {
                var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);
                var commandIssuer = Context.Guild.GetUser(Context.User.Id);
                if (campaign.UserId != Context.User.Id && !commandIssuer.GuildPermissions.Administrator)
                    return GameMasterResult.ErrorResult("you do not have permission to add players to this campaign. You must either be the Game Master of this campaign or a Server Administrator.");
                var playersToAdd = new List<SocketGuildUser>();
                foreach (var guildUser in guildUsers)
                {
                    if (campaign.CampaignUsers.FirstOrDefault(cu => cu.UserId == guildUser.Id) != null)
                        await ReplyAsync($"'{guildUser.Username}' is already a player in this campaign, so I'll skip them.");
                    else
                        playersToAdd.Add(guildUser);
                }

                campaign = await _campaignService.AddPlayers(campaign.Id, playersToAdd);
                
                var campaignRole = Context.Guild.Roles.FirstOrDefault(role => role.Id == campaign.PlayerRoleId);
                if (campaignRole == null)
                    return GameMasterResult.ErrorResult("no player role exists for this campaign.");

                foreach (var newPlayer in playersToAdd) await newPlayer.AddRoleAsync(campaignRole);

                await ReplyAsync("Successfully added new players to this campaign.", embed: EmbedBuilder.CampaignInfo(campaign));
                return GameMasterResult.SuccessResult();
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult(e.Message);
            }
        }
        
        [RequireRole("Whitelisted")]
        [Command("remove-players")]
        [Summary("Removes one or more players from this campaign.")]
        public async Task<RuntimeResult> RemovePlayersAsync(
            [Summary("The usernames/nicknames of the players in this campaign that you want to remove.")] params string[] names)
        {
            var guildUsers = new List<SocketGuildUser>();
            foreach (var name in names)
            {
                var guildUser = Context.Guild.Users.FirstOrDefault(user =>
                    string.Equals(user.Username, name, StringComparison.CurrentCultureIgnoreCase) ||
                    string.Equals(user.Nickname, name, StringComparison.CurrentCultureIgnoreCase));
                if (guildUser == null)
                    await ReplyAsync($"I couldn't find a user by the name of '{name}' in this server, so I'll skip them.");
                else
                    guildUsers.Add(guildUser);
            }
            try
            {
                var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);
                var commandIssuer = Context.Guild.GetUser(Context.User.Id);
                if (campaign.UserId != Context.User.Id && !commandIssuer.GuildPermissions.Administrator)
                    return GameMasterResult.ErrorResult("you do not have permission to remove players from this campaign. You must either be the Game Master of this campaign or a Server Administrator.");
                var playersToRemove = new List<SocketGuildUser>();
                foreach (var guildUser in guildUsers)
                {
                    if (campaign.CampaignUsers.FirstOrDefault(cu => cu.UserId == guildUser.Id) == null)
                        await ReplyAsync($"'{guildUser.Username}' is not a player in this campaign, so I'll skip them.");
                    else
                        playersToRemove.Add(guildUser);
                }

                campaign = await _campaignService.RemovePlayers(campaign.Id, playersToRemove);
                
                var campaignRole = Context.Guild.Roles.FirstOrDefault(role => role.Id == campaign.PlayerRoleId);
                if (campaignRole == null)
                    return GameMasterResult.ErrorResult("no player role exists for this campaign.");

                
                foreach (var newPlayer in playersToRemove) await newPlayer.RemoveRoleAsync(campaignRole);

                await ReplyAsync("Successfully removed players from this campaign.", embed: EmbedBuilder.CampaignInfo(campaign));
                return GameMasterResult.SuccessResult();
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult(e.Message);
            }
        }

        [RequireRole("Whitelisted")]
        [Command("set-url")]
        [Summary("Set the game URL of this campaign.")]
        public async Task<RuntimeResult> SetUrlAsync(
            [Summary("The URL of the campaign.")] string url)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
                return GameMasterResult.ErrorResult("the URL you entered is not valid.");
            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);
            campaign = await _campaignService.UpdateUrl(campaign.Id, url);
            await ReplyAsync("Successfully set the URL of this campaign.", embed: EmbedBuilder.CampaignInfo(campaign));
            return GameMasterResult.SuccessResult();
        }
        
        [RequireRole("Whitelisted")]
        [Command("remove", RunMode = RunMode.Async), Alias("delete")]
        [Summary("Deletes this campaign from the server, including channels and roles.")]
        public async Task<RuntimeResult> RemoveAsync()
        {
            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);
            var commandIssuer = Context.Guild.GetUser(Context.User.Id);
            if (campaign.UserId != Context.User.Id && !commandIssuer.GuildPermissions.Administrator)                
                return GameMasterResult.ErrorResult("you do not have permission to remove this campaign. You must either be the Game Master of this campaign or a Server Administrator.");
            
            var message = await ReplyAsync("Removing campaign channel and roles...");

            var textChannel = Context.Guild.TextChannels.FirstOrDefault(channel => channel.Id == campaign.TextChannelId);
            if (textChannel != null) await Context.Guild.GetTextChannel(textChannel.Id).DeleteAsync();
        
            var voiceChannel = Context.Guild.VoiceChannels.FirstOrDefault(channel => channel.Id == campaign.VoiceChannelId);
            if (voiceChannel != null) await Context.Guild.GetVoiceChannel(voiceChannel.Id).DeleteAsync();
        
            var campaignRole = Context.Guild.Roles.FirstOrDefault(role => role.Id == campaign.PlayerRoleId);
            if (campaignRole != null) await Context.Guild.GetRole(campaignRole.Id).DeleteAsync();
            var gmRole = Context.Guild.Roles.FirstOrDefault(role => role.Id == campaign.GameMasterRoleId);
            if (gmRole != null) await Context.Guild.GetRole(gmRole.Id).DeleteAsync();

            try
            {
                await _campaignService.Remove(campaign.Id);
                await message.ModifyAsync(m => m.Content = "Campaign removed successfully!");
                return GameMasterResult.SuccessResult();
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult(e.Message);
            }
        }

        [Command("info"), Alias("details")]
        [Summary("Returns all information about this campaign.")]
        public async Task<RuntimeResult> InfoAsync()
        {
            try
            {
                var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);
                await ReplyAsync(embed: EmbedBuilder.CampaignSummary(campaign));
                return GameMasterResult.SuccessResult();
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult(e.Message);
            }
        }
        
        [Command("server")]
        [Summary("Returns information about all the campaigns on this server.")]
        public async Task<RuntimeResult> ServerAsync()
        {
            try
            {
                var campaigns = await _campaignService.GetForServer(Context.Guild.Id);
                foreach (var campaign in campaigns) await ReplyAsync(embed: EmbedBuilder.CampaignSummary(campaign));
                return GameMasterResult.SuccessResult();
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult(e.Message);
            }
        }
    }
}
