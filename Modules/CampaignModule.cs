using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using GameMasterBot.Constants;
using GameMasterBot.Embeds;
using GameMasterBot.Extensions;
using GameMasterBot.Models.Entities;
using GameMasterBot.Services;

namespace GameMasterBot.Modules
{
    [RequireContext(ContextType.Guild)]
    [Group("campaign", "Commands for managing the campaigns on your server.")]
    public class CampaignModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly CampaignService _campaignService;
        
        public CampaignModule(CampaignService campaignService) => _campaignService = campaignService;

        [RequireRoleOrAdmin("Whitelisted")]
        [SlashCommand("create", "Create a new campaign on this server, including new channels and roles.")]
        public async Task<RuntimeResult> CreateCampaignAsync(
            [Summary("campaign-name", "The name of your new campaign.")] string campaignName,
            [Summary("game-system", "The name of the game system you will use for your new campaign.")] string gameSystem)
        {

            // TODO move validation elsewhere
            var channelRegex = new Regex(ChannelValidationConstants.NameRegexPattern);
            
            if (campaignName.Length > ChannelValidationConstants.NameMaxLength)
            {
                return CommandResult.FromError($"The name of your campaign must be less than {ChannelValidationConstants.NameMaxLength} characters.");
            }

            if (!channelRegex.IsMatch(campaignName))
            {
                return CommandResult.FromError("The name of your campaign must only contain alphanumeric characters and spaces.");
            }

            if (gameSystem.Length > ChannelValidationConstants.NameMaxLength)
            {
                return CommandResult.FromError($"The name of your campaign's game system must be less than {ChannelValidationConstants.NameMaxLength} characters long.");
            }

            if (!channelRegex.IsMatch(campaignName))
            {
                return CommandResult.FromError("The name of your campaign's game system must only contain alphanumeric characters and spaces.");
            }

            var textChannelName = campaignName.ToLower().Replace(' ', '-');
            var campaignExisting = (await _campaignService.GetForServer(Context.Guild.Id)).FirstOrDefault(c => c.Name == campaignName);
            if (campaignExisting != null)
            {
                return CommandResult.FromError("A campaign with this name already exists on this server.");
            }

            await DeferAsync(ephemeral: true);

            // TODO: Move resource creation elsewhere
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
            var playerRole = Context.Guild.Roles.FirstOrDefault(role => role.Name == $"Player: {campaignName}") ??
                             (IRole)Context.Guild.CreateRoleAsync($"Player: {campaignName}", null, roleColor, false, true).Result;
    
            var gmRole = Context.Guild.Roles.FirstOrDefault(role => role.Name == $"Game Master: {campaignName}") ??
                             (IRole)Context.Guild.CreateRoleAsync($"Game Master: {campaignName}", null, roleColor, false, true).Result;
    
            // Create the category channel for this campaign's system if one does not already exist
            var campaignCategoryChannel = Context.Guild.CategoryChannels.FirstOrDefault(cat => cat.Name == gameSystem) ??
                                              (ICategoryChannel)Context.Guild.CreateCategoryChannelAsync(gameSystem).Result;
    
            // Create the text channel for this campaign if one does not exist
            var campaignTextChannel = Context.Guild.TextChannels.FirstOrDefault(chan => chan.Name == textChannelName) ??
                                      (ITextChannel)Context.Guild.CreateTextChannelAsync(textChannelName, channel =>
                                      {
                                          channel.CategoryId = campaignCategoryChannel.Id;
                                          channel.Topic = $"Channel for discussing the {gameSystem} campaign '{campaignName}'.";
                                      }).Result;
    
            // Set the permissions on the campaign's text channel
            await campaignTextChannel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, new OverwritePermissions(readMessageHistory: PermValue.Deny, sendMessages: PermValue.Deny, viewChannel: PermValue.Deny));
            await campaignTextChannel.AddPermissionOverwriteAsync(playerRole, new OverwritePermissions(sendMessages: PermValue.Allow, readMessageHistory: PermValue.Allow, viewChannel: PermValue.Allow, attachFiles: PermValue.Allow, addReactions: PermValue.Allow));
            await campaignTextChannel.AddPermissionOverwriteAsync(gmRole, new OverwritePermissions(sendMessages: PermValue.Allow, readMessageHistory: PermValue.Allow, manageMessages: PermValue.Allow, manageChannel: PermValue.Allow, viewChannel: PermValue.Allow, attachFiles: PermValue.Allow));
    
            // Create the voice channel for this campaign if one does not exist
            var campaignVoiceChannel = Context.Guild.VoiceChannels.FirstOrDefault(chan => chan.Name == campaignName) ??
                                       (IVoiceChannel)Context.Guild.CreateVoiceChannelAsync(campaignName,
                                           channel => channel.CategoryId = campaignCategoryChannel.Id).Result;
    
            // Set the permissions on the campaign's voice channel
            await campaignVoiceChannel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, new OverwritePermissions(connect: PermValue.Deny, viewChannel: PermValue.Deny));
            await campaignVoiceChannel.AddPermissionOverwriteAsync(playerRole, new OverwritePermissions(connect: PermValue.Allow, viewChannel: PermValue.Allow, speak: PermValue.Allow, useVoiceActivation: PermValue.Allow));
            await campaignVoiceChannel.AddPermissionOverwriteAsync(gmRole, new OverwritePermissions(connect: PermValue.Allow, manageChannel: PermValue.Allow, viewChannel: PermValue.Allow, speak: PermValue.Allow, useVoiceActivation: PermValue.Allow));
    
            // Add the gm role to the game master
            await Context.Guild.Users.First(user => user.Id == Context.User.Id).AddRoleAsync(gmRole);
            
            // TODO: Create a DTO for this
            var campaign = await _campaignService.Create(campaignName, gameSystem, Context.User, Context.Guild, campaignTextChannel.Id, campaignVoiceChannel.Id, playerRole.Id, gmRole.Id);

            await ModifyOriginalResponseAsync(m =>
            {
                m.Content = $"Your campaign has been created successfully! You should head over to <#{campaignTextChannel.Id}> and add your players with the '/campaign add-player' command.";
                m.Embed = BotEmbeds.CampaignInfo(campaign);
            });
            return CommandResult.AsSuccess();
        }
        
        [RequireRoleOrAdmin("Whitelisted")]
        [SlashCommand("add-player", "Add a new player to this campaign.")]
        public async Task<RuntimeResult> AddPlayersAsync(
            [Summary("player", "The person that you want to add as a player to this campaign.")] SocketGuildUser mentionedUser)
        {
            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);
            if (campaign == null)
            {
                return CommandResult.FromError("You must be in your campaign's text channel to add a new player.");
            }

            if (mentionedUser.Id == campaign.GameMaster.User.DiscordId)
            {
                return CommandResult.FromError($"'{mentionedUser.Username}' is already the game master for this campaign, so you cannot add them.");
            }

            var commandIssuer = Context.Guild.GetUser(Context.User.Id);
            if (campaign.GameMaster.User.DiscordId != Context.User.Id && !commandIssuer.GuildPermissions.Administrator)
            {
                return CommandResult.FromError("You do not have permission to add players to this campaign. You must either be the game master of this campaign or a server administrator.");
            }

            var foundPlayer = campaign.Players.FirstOrDefault(cu => cu.User.DiscordId == mentionedUser.Id);
            if (foundPlayer != null)
            {
                return CommandResult.FromError($"'{mentionedUser.Username}' is already a player in this campaign.");
            }

            campaign = await _campaignService.AddPlayer(campaign.Id, mentionedUser);
            
            var campaignRole = Context.Guild.Roles.FirstOrDefault(role => role.Id == campaign.PlayerRoleId);
            if (campaignRole == null)
            {
                return CommandResult.FromError("I couldn't find the player role for this campaign in this server. You can regenerate the roles for this campaign by using the '/campaign create' command again and entering the same campaign name and game system.");
            }

            await mentionedUser.AddRoleAsync(campaignRole);
            
            await RespondAsync($"Successfully added {mentionedUser.Username} to this campaign as a new player.", embed: BotEmbeds.CampaignInfo(campaign));
            return CommandResult.AsSuccess();
        }
        
        [RequireRoleOrAdmin("Whitelisted")]
        [SlashCommand("remove-player", "Removes a player from this campaign.")]
        public async Task<RuntimeResult> RemovePlayersAsync(
            [Summary("The person that you want to remove as a player from this campaign.")] SocketGuildUser mentionedUser)
        {
            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);
            if (campaign == null) return CommandResult.FromError("You must be in your campaign's text channel to remove a player.");

            var commandIssuer = Context.Guild.GetUser(Context.User.Id);
            if (campaign.GameMaster.User.DiscordId != Context.User.Id && !commandIssuer.GuildPermissions.Administrator)
            {
                return CommandResult.FromError("You do not have permission to remove players from this campaign. You must either be the game master of this campaign or a server administrator.");
            }

            var foundPlayer = campaign.Players.FirstOrDefault(cu => cu.User.DiscordId == mentionedUser.Id);
            if (foundPlayer != null)
            {
                return CommandResult.FromError($"'{mentionedUser.Username}' is not a player in this campaign.");
            }
    
            campaign = await _campaignService.RemovePlayer(campaign.Id, mentionedUser);
            
            var campaignRole = Context.Guild.Roles.FirstOrDefault(role => role.Id == campaign.PlayerRoleId);
            if (campaignRole == null)
            {
                return CommandResult.FromError("I couldn't find the player role for this campaign in this server. You can regenerate the roles for this campaign by using the '/campaign create' command again and entering the same campaign name and game system.");
            }

            await mentionedUser.RemoveRoleAsync(campaignRole);

            await RespondAsync($"Successfully removed {mentionedUser.Username} from this campaign.", embed: BotEmbeds.CampaignInfo(campaign));
            return CommandResult.AsSuccess();
        }
        
        [RequireRoleOrAdmin("Whitelisted")]
        [SlashCommand("add-url", "Add a URL for this campaign where players can access the game online.")]
        public async Task<RuntimeResult> AddUrlAsync(
            [Summary("The URL of the campaign.")] string url)
        {
            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);
            if (campaign == null)
            {
                return CommandResult.FromError("You must be in your campaign's text channel to set the game URL.");
            }

            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                return CommandResult.FromError("The URL you entered is not valid a valid URL.");
            }

            campaign.Url = url;
            campaign = await _campaignService.Update(campaign);
            
            await RespondAsync("Successfully set the URL for this campaign.", embed: BotEmbeds.CampaignInfo(campaign));
            return CommandResult.AsSuccess();
        }
        
        [RequireRoleOrAdmin("Whitelisted")]
        [SlashCommand("assign-gm", "Assign a new game master for this campaign.")]
        public async Task<RuntimeResult> SetGameMasterAsync(
            [Summary("The person that you want to assign as the new game master for this campaign.")] SocketGuildUser mentionedUser)
        {
            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);
            if (campaign == null)
            {
                return CommandResult.FromError("You must be in your campaign's text channel to assign a new game master.");
            }

            var gmRole = Context.Guild.Roles.FirstOrDefault(role => role.Id == campaign.GameMasterRoleId);
            if (gmRole == null)
            {
                return CommandResult.FromError("I couldn't find the game master role for your campaign on this server. You can regenerate the roles for this campaign by using the '/campaign create' command again and entering the same campaign name and game system.");
            }

            if (campaign.GameMaster.User.DiscordId == mentionedUser.Id)
            {
                return CommandResult.FromError($"'{mentionedUser.Username}' is already the game master for this campaign!");
            }

            var currentGmDiscord = Context.Guild.GetUser(campaign.GameMaster.User.DiscordId);
            if (currentGmDiscord != null)
            {
                await currentGmDiscord.RemoveRoleAsync(gmRole);
            }

            var player= campaign.Players.SingleOrDefault(cu => cu.User.DiscordId == mentionedUser.Id);
            if (player != null)
            {
                var playerRole = Context.Guild.Roles.FirstOrDefault(role => role.Id == campaign.PlayerRoleId);
                if (playerRole == null)
                {
                    return CommandResult.FromError("I couldn't find the player role for this campaign in this server. You can regenerate the roles for this campaign by using the '/campaign create' command again and entering the same campaign name and game system.");
                }

                await mentionedUser.RemoveRoleAsync(playerRole);
                await mentionedUser.AddRoleAsync(gmRole);
                campaign.Players.Remove(player);
            }
            else await mentionedUser.AddRoleAsync(gmRole);
        
            var newGameMasterUser = await _campaignService.GetUserByDiscordUser(mentionedUser);
            campaign.GameMaster = new GameMaster { User = newGameMasterUser };
            campaign = await _campaignService.Update(campaign);
            
            await RespondAsync($"Successfully assigned {mentionedUser.Username} as the game master for this campaign.", embed: BotEmbeds.CampaignInfo(campaign));
            return CommandResult.AsSuccess();
        }
        
        [RequireRoleOrAdmin("Whitelisted")]
        [SlashCommand("delete", "Deletes this campaign from the server, including channels and roles.")]
        public async Task<RuntimeResult> DeleteCampaignAsync()
        {
            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);
            if (campaign == null)
            {
                return CommandResult.FromError("You must be in your campaign's text channel to delete it.");
            }

            var commandIssuer = Context.Guild.GetUser(Context.User.Id);
            if (campaign.GameMaster.User.DiscordId != Context.User.Id && !commandIssuer.GuildPermissions.Administrator)
            {
                return CommandResult.FromError("You do not have permission to delete this campaign. You must either be the game master of this campaign or a server administrator.");
            }

            await DeferAsync(ephemeral: true);
        
            var textChannel = Context.Guild.TextChannels.FirstOrDefault(channel => channel.Id == campaign.TextChannelId);
            if (textChannel != null)
            {
                await Context.Guild.GetTextChannel(textChannel.Id).DeleteAsync();
            }

            var voiceChannel = Context.Guild.VoiceChannels.FirstOrDefault(channel => channel.Id == campaign.VoiceChannelId);
            if (voiceChannel != null)
            {
                await Context.Guild.GetVoiceChannel(voiceChannel.Id).DeleteAsync();
            }

            var campaignRole = Context.Guild.Roles.FirstOrDefault(role => role.Id == campaign.PlayerRoleId);
            if (campaignRole != null)
            {
                await Context.Guild.GetRole(campaignRole.Id).DeleteAsync();
            }

            var gmRole = Context.Guild.Roles.FirstOrDefault(role => role.Id == campaign.GameMasterRoleId);
            if (gmRole != null)
            {
                await Context.Guild.GetRole(gmRole.Id).DeleteAsync();
            }
            
            await _campaignService.Remove(campaign.Id);
            
            await ModifyOriginalResponseAsync(m => m.Content = "Campaign successfully deleted!");
            return CommandResult.AsSuccess();
        }
        
        [SlashCommand("info", "Displays all information about this campaign.")]
        public async Task<RuntimeResult> CampaignInfoAsync()
        {
            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);
            if (campaign == null)
            {
                return CommandResult.FromError("You must be in your campaign's text channel to display info on it.");
            }

            await RespondAsync(embed: BotEmbeds.CampaignSummary(campaign));
            return CommandResult.AsSuccess();
        }
        
        [SlashCommand("server", "Displays information about all the campaigns on this server.")]
        public async Task<RuntimeResult> CampaignServerInfoAsync()
        {
            var campaigns = await _campaignService.GetForServer(Context.Guild.Id);
            var campaignEmbeds = campaigns.Select(BotEmbeds.CampaignSummary).ToArray();
            
            await RespondAsync(embeds: campaignEmbeds);
            return CommandResult.AsSuccess();
        }
     }
}
