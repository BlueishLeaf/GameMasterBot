using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Interactions;
using Discord.WebSocket;
using GameMasterBot.Constants;
using GameMasterBot.DTO;
using GameMasterBot.Embeds;
using GameMasterBot.Extensions;
using GameMasterBot.Messages;
using GameMasterBot.Models.Entities;
using GameMasterBot.Services;
using GameMasterBot.Utils;

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
            // Defer response as it takes longer than 3 seconds to create the campaign resources
            await DeferAsync(ephemeral: true);

            var commandValidationError = await ValidateCreateCampaignCommand(campaignName, gameSystem);
            if (commandValidationError != null)
                return CommandResult.FromError(commandValidationError.ErrorMessage);

            var entityManager = new CampaignSocketEntityManager(Context);
            var campaignEntities = await entityManager.CreateNewCampaign(campaignName, gameSystem);

            var createCampaignDto = new CreateCampaignDto(
                campaignName,
                gameSystem,
                Context.User,
                Context.Guild,
                campaignEntities.TextChannelId,
                campaignEntities.VoiceChannelId,
                campaignEntities.PlayerRoleId,
                campaignEntities.GameMasterRoleId);
            var campaign = await _campaignService.Create(createCampaignDto);

            await ModifyOriginalResponseAsync(message =>
            {
                message.Content = CampaignResponseMessages.CampaignSuccessfullyCreated(campaign.TextChannelId);
                message.Embed = BotEmbeds.CampaignInfo(campaign);
            });
            return CommandResult.AsSuccess();
        }

        private async Task<CommandValidationError?> ValidateCreateCampaignCommand(string campaignName, string gameSystem)
        {
            if (campaignName.Length > CampaignValidationConstants.NameMaxLength)
                return CampaignValidationMessages.InvalidNameLength();

            var channelRegex = new Regex(CampaignValidationConstants.NameRegexPattern);
            if (!channelRegex.IsMatch(campaignName))
                return CampaignValidationMessages.InvalidNamePattern();

            if (gameSystem.Length > CampaignValidationConstants.NameMaxLength)
                return CampaignValidationMessages.InvalidSystemLength();

            if (!channelRegex.IsMatch(gameSystem))
                return CampaignValidationMessages.InvalidSystemPattern();

            var guildCampaigns = await _campaignService.GetAllByGuildId(Context.Guild.Id);
            return guildCampaigns.Any(c => c.Name == campaignName) ?
                CampaignValidationMessages.CampaignAlreadyExists() :
                null;
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
            var campaigns = await _campaignService.GetAllByGuildId(Context.Guild.Id);
            var campaignEmbeds = campaigns.Select(BotEmbeds.CampaignSummary).ToArray();
            
            await RespondAsync(embeds: campaignEmbeds);
            return CommandResult.AsSuccess();
        }
     }
}
