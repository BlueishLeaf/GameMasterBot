using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
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
        private readonly UserService _userService;
        
        public CampaignModule(CampaignService campaignService, UserService userService)
        {
            _campaignService = campaignService;
            _userService = userService;
        }

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
                Context.User.Id,
                Context.Guild.Id,
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

        private async Task<CommandValidationError> ValidateCreateCampaignCommand(string campaignName, string gameSystem)
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
            var commandValidationError = await ValidateAddPlayerCommand(mentionedUser);
            if (commandValidationError != null)
                return CommandResult.FromError(commandValidationError.ErrorMessage);
            
            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);
            campaign = await _campaignService.AddPlayer(campaign.Id, mentionedUser.Id);

            var campaignRole = Context.Guild.Roles.First(role => role.Id == campaign.PlayerRoleId);
            await mentionedUser.AddRoleAsync(campaignRole);
            
            await RespondAsync($"Successfully added {mentionedUser.Username} to this campaign as a new player.", embed: BotEmbeds.CampaignInfo(campaign));
            return CommandResult.AsSuccess();
        }

        private async Task<CommandValidationError> ValidateAddPlayerCommand(IUser mentionedUser)
        {
            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);
            if (campaign == null)
                return CommonValidationMessages.NotInCampaignChannel();

            if (mentionedUser.Id == campaign.GameMaster.User.DiscordId)
                return CampaignValidationMessages.CannotAddGameMaster(mentionedUser.Username);

            var commandIssuer = Context.Guild.GetUser(Context.User.Id);
            if (campaign.GameMaster.User.DiscordId != Context.User.Id && !commandIssuer.GuildPermissions.Administrator)
                return CommonValidationMessages.NotGameMasterOrAdmin();
            
            var campaignRole = Context.Guild.Roles.FirstOrDefault(role => role.Id == campaign.PlayerRoleId);
            if (campaignRole == null)
                return CampaignValidationMessages.NoPlayerRole();

            var foundPlayer = campaign.Players.FirstOrDefault(cu => cu.User.DiscordId == mentionedUser.Id);
            return foundPlayer != null ?
                CampaignValidationMessages.CannotAddExistingPlayer(mentionedUser.Username) :
                null;
        }

        [RequireRoleOrAdmin("Whitelisted")]
        [SlashCommand("remove-player", "Removes a player from this campaign.")]
        public async Task<RuntimeResult> RemovePlayersAsync(
            [Summary("player", "The person that you want to remove as a player from this campaign.")] SocketGuildUser mentionedUser)
        {
            var commandValidationError = await ValidateRemovePlayerCommand(mentionedUser);
            if (commandValidationError != null)
                return CommandResult.FromError(commandValidationError.ErrorMessage);
            
            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);
            campaign = await _campaignService.RemovePlayer(campaign.Id, mentionedUser.Id);
            
            var campaignRole = Context.Guild.Roles.First(role => role.Id == campaign.PlayerRoleId);
            await mentionedUser.RemoveRoleAsync(campaignRole);

            await RespondAsync($"Successfully removed {mentionedUser.Username} from this campaign.", embed: BotEmbeds.CampaignInfo(campaign));
            return CommandResult.AsSuccess();
        }

        private async Task<CommandValidationError> ValidateRemovePlayerCommand(IUser mentionedUser)
        {
            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);
            if (campaign == null)
                return CommonValidationMessages.NotInCampaignChannel();

            var commandIssuer = Context.Guild.GetUser(Context.User.Id);
            if (campaign.GameMaster.User.DiscordId != Context.User.Id && !commandIssuer.GuildPermissions.Administrator)
                return CommonValidationMessages.NotGameMasterOrAdmin();

            var foundPlayer = campaign.Players.FirstOrDefault(cu => cu.User.DiscordId == mentionedUser.Id);
            if (foundPlayer == null)
                return CampaignValidationMessages.CannotRemoveNonPlayer(mentionedUser.Username);
            
            var campaignRole = Context.Guild.Roles.FirstOrDefault(role => role.Id == campaign.PlayerRoleId);
            return campaignRole == null ?
                CampaignValidationMessages.NoPlayerRole() :
                null;
        }
        
        [RequireRoleOrAdmin("Whitelisted")]
        [SlashCommand("set-url", "Set the URL for this campaign where players can access the game online.")]
        public async Task<RuntimeResult> SetUrlAsync(
            [Summary("url", "The URL of the campaign.")] string url)
        {
            var commandValidationError = await ValidateSetUrlCommand(url);
            if (commandValidationError != null)
                return CommandResult.FromError(commandValidationError.ErrorMessage);
            
            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);

            campaign.Url = url;
            campaign = await _campaignService.Update(campaign);
            
            await RespondAsync("Successfully set the URL for this campaign.", embed: BotEmbeds.CampaignInfo(campaign));
            return CommandResult.AsSuccess();
        }

        private async Task<CommandValidationError> ValidateSetUrlCommand(string url)
        {
            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);
            if (campaign == null)
                return CommonValidationMessages.NotInCampaignChannel();
            
            var commandIssuer = Context.Guild.GetUser(Context.User.Id);
            if (campaign.GameMaster.User.DiscordId != Context.User.Id && !commandIssuer.GuildPermissions.Administrator)
                return CommonValidationMessages.NotGameMasterOrAdmin();

            return !Uri.IsWellFormedUriString(url, UriKind.Absolute) ? CampaignValidationMessages.InvalidURL() : null;
        }
        
        [RequireRoleOrAdmin("Whitelisted")]
        [SlashCommand("set-game-master", "Assign a new game master for this campaign.")]
        public async Task<RuntimeResult> SetGameMasterAsync(
            [Summary("game-master", "The person that you want to assign as the new game master for this campaign.")] SocketGuildUser mentionedUser)
        {
            var commandValidationError = await ValidateSetGameMaster(mentionedUser);
            if (commandValidationError != null)
                return CommandResult.FromError(commandValidationError.ErrorMessage);

            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);

            var gmRole = Context.Guild.Roles.First(r => r.Id == campaign.GameMasterRoleId);
            var currentGmDiscord = Context.Guild.GetUser(campaign.GameMaster.User.DiscordId);
            if (currentGmDiscord != null) await currentGmDiscord.RemoveRoleAsync(gmRole);

            var playerRole = Context.Guild.Roles.First(r => r.Id == campaign.PlayerRoleId);
            var player = campaign.Players.SingleOrDefault(cu => cu.User.DiscordId == mentionedUser.Id);
            if (player != null)
            {
                await mentionedUser.RemoveRoleAsync(playerRole);
                await mentionedUser.AddRoleAsync(gmRole);
                campaign.Players.Remove(player);
            }
            else await mentionedUser.AddRoleAsync(gmRole);
        
            var newGameMasterUser = await _userService.GetByDiscordUserId(mentionedUser.Id);
            campaign.GameMaster = new GameMaster { User = newGameMasterUser };
            campaign = await _campaignService.Update(campaign);
            
            await RespondAsync($"Successfully assigned {mentionedUser.Username} as the game master for this campaign.", embed: BotEmbeds.CampaignInfo(campaign));
            return CommandResult.AsSuccess();
        }

        private async Task<CommandValidationError> ValidateSetGameMaster(IUser newGameMaster)
        {
            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);
            if (campaign == null)
                return CommonValidationMessages.NotInCampaignChannel();
            
            var commandIssuer = Context.Guild.GetUser(Context.User.Id);
            if (campaign.GameMaster.User.DiscordId != Context.User.Id && !commandIssuer.GuildPermissions.Administrator)
                return CommonValidationMessages.NotGameMasterOrAdmin();

            if (campaign.GameMaster.User.DiscordId == newGameMaster.Id)
                return CampaignValidationMessages.CannotSetCurrentGameMaster(newGameMaster.Username);

            if (Context.Guild.Roles.All(role => role.Id != campaign.GameMasterRoleId))
                return CampaignValidationMessages.NoGameMasterRole();

            return Context.Guild.Roles.All(role => role.Id != campaign.PlayerRoleId) ?
                CampaignValidationMessages.NoPlayerRole() :
                null;
        }
        
        [RequireRoleOrAdmin("Whitelisted")]
        [SlashCommand("delete", "Deletes this campaign from the server, including channels and roles.")]
        public async Task<RuntimeResult> DeleteCampaignAsync()
        {
            await DeferAsync(ephemeral: true);
            
            var commandValidationError = await ValidateDeleteCampaignCommand();
            if (commandValidationError != null)
                return CommandResult.FromError(commandValidationError.ErrorMessage);

            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);

            var textChannel = Context.Guild.TextChannels.FirstOrDefault(channel => channel.Id == campaign.TextChannelId);
            if (textChannel != null) await Context.Guild.GetTextChannel(textChannel.Id).DeleteAsync();

            var voiceChannel = Context.Guild.VoiceChannels.FirstOrDefault(channel => channel.Id == campaign.VoiceChannelId);
            if (voiceChannel != null) await Context.Guild.GetVoiceChannel(voiceChannel.Id).DeleteAsync();

            var campaignRole = Context.Guild.Roles.FirstOrDefault(role => role.Id == campaign.PlayerRoleId);
            if (campaignRole != null) await Context.Guild.GetRole(campaignRole.Id).DeleteAsync();

            var gmRole = Context.Guild.Roles.FirstOrDefault(role => role.Id == campaign.GameMasterRoleId);
            if (gmRole != null) await Context.Guild.GetRole(gmRole.Id).DeleteAsync();

            await _campaignService.Remove(campaign.Id);
            
            return CommandResult.AsSuccess();
        }

        private async Task<CommandValidationError> ValidateDeleteCampaignCommand()
        {
            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);
            if (campaign == null)
                return CommonValidationMessages.NotInCampaignChannel();

            var commandIssuer = Context.Guild.GetUser(Context.User.Id);
            var notGmOrAdmin =
                campaign.GameMaster.User.DiscordId != Context.User.Id &&
                !commandIssuer.GuildPermissions.Administrator;
            return notGmOrAdmin
                ? CommonValidationMessages.NotGameMasterOrAdmin()
                : null;
        }
        
        [SlashCommand("info", "Displays all information about this campaign.")]
        public async Task<RuntimeResult> CampaignInfoAsync()
        {
            var commandValidationError = await ValidateCampaignInfoCommand();
            if (commandValidationError != null)
                return CommandResult.FromError(commandValidationError.ErrorMessage);
                
            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);

            await RespondAsync(embed: BotEmbeds.CampaignSessionSummary(campaign));
            return CommandResult.AsSuccess();
        }

        private async Task<CommandValidationError> ValidateCampaignInfoCommand()
        {
            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);
            return campaign == null ?
                CommonValidationMessages.NotInCampaignChannel() :
                null;
        }
    }
}
