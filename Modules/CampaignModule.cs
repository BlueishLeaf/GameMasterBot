using System.Threading.Tasks;
using Discord.Interactions;
using Discord.WebSocket;
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
        private readonly CampaignValidationService _validationService;
        private readonly CampaignService _campaignService;
        private readonly UserService _userService;
        
        public CampaignModule(CampaignValidationService validationService, CampaignService campaignService, UserService userService)
        {
            _validationService = validationService;
            _campaignService = campaignService;
            _userService = userService;
        }

        [RequireRoleOrAdmin("Whitelisted")]
        [SlashCommand("create", "Create a new campaign on this server, including new channels and roles.")]
        public async Task<RuntimeResult> CreateCampaignAsync(
            [Summary("campaign-name", "The name of your new campaign.")] string campaignName,
            [Summary("game-system", "The name of the game system you will use for your new campaign.")] string gameSystem)
        {
            await DeferAsync(ephemeral: true);

            var createSocketCampaignDto = new CreateSocketCampaignDto(campaignName, gameSystem);

            var commandValidationError = await _validationService.ValidateCreateCampaignCommand(Context, createSocketCampaignDto);
            if (commandValidationError != null) return CommandResult.FromError(commandValidationError.ErrorMessage);

            var createCampaignDto = await CampaignSocketUtils.CreateSocketCampaign(Context, createSocketCampaignDto);
            var campaign = await _campaignService.Create(createCampaignDto);

            await ModifyOriginalResponseAsync(message =>
            {
                message.Content = CampaignResponseMessages.CampaignSuccessfullyCreated(campaign.TextChannelId);
                message.Embed = BotEmbeds.CampaignInfo(campaign);
            });
            return CommandResult.AsSuccess();
        }

        [RequireRoleOrAdmin("Whitelisted")]
        [SlashCommand("add-player", "Add a new player to this campaign.")]
        public async Task<RuntimeResult> AddPlayerAsync(
            [Summary("player", "The person that you want to add as a player to this campaign.")] SocketGuildUser newPlayer)
        {
            var commandValidationError = await _validationService.ValidateAddPlayerCommand(Context, newPlayer);
            if (commandValidationError != null) return CommandResult.FromError(commandValidationError.ErrorMessage);
            
            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);
            campaign = await _campaignService.AddPlayer(campaign.Id, newPlayer.Id);
            
            await CampaignSocketUtils.AddPlayer(Context, newPlayer, campaign.PlayerRoleId);
            
            await RespondAsync($"Successfully added {newPlayer.Username} to this campaign as a new player.", embed: BotEmbeds.CampaignInfo(campaign));
            return CommandResult.AsSuccess();
        }

        [RequireRoleOrAdmin("Whitelisted")]
        [SlashCommand("remove-player", "Removes a player from this campaign.")]
        public async Task<RuntimeResult> RemovePlayerAsync(
            [Summary("player", "The person that you want to remove as a player from this campaign.")] SocketGuildUser playerToRemove)
        {
            var commandValidationError = await _validationService.ValidateRemovePlayerCommand(Context, playerToRemove);
            if (commandValidationError != null) return CommandResult.FromError(commandValidationError.ErrorMessage);
            
            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);
            campaign = await _campaignService.RemovePlayer(campaign.Id, playerToRemove.Id);

            await CampaignSocketUtils.RemovePlayer(Context, playerToRemove, campaign.PlayerRoleId);

            await RespondAsync($"Successfully removed {playerToRemove.Username} from this campaign.", embed: BotEmbeds.CampaignInfo(campaign));
            return CommandResult.AsSuccess();
        }

        [RequireRoleOrAdmin("Whitelisted")]
        [SlashCommand("set-url", "Set the URL for this campaign where players can access the game online.")]
        public async Task<RuntimeResult> SetUrlAsync(
            [Summary("url", "The URL of the campaign.")] string url)
        {
            var commandValidationError = await _validationService.ValidateSetUrlCommand(Context, url);
            if (commandValidationError != null) return CommandResult.FromError(commandValidationError.ErrorMessage);
            
            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);
            campaign.Url = url;
            campaign = await _campaignService.Update(campaign);
            
            await RespondAsync("Successfully set the URL for this campaign.", embed: BotEmbeds.CampaignInfo(campaign));
            return CommandResult.AsSuccess();
        }

        [RequireRoleOrAdmin("Whitelisted")]
        [SlashCommand("set-game-master", "Assign a new game master for this campaign.")]
        public async Task<RuntimeResult> SetGameMasterAsync(
            [Summary("game-master", "The person that you want to assign as the new game master for this campaign.")] SocketGuildUser newGameMaster)
        {
            var commandValidationError = await _validationService.ValidateSetGameMaster(Context, newGameMaster);
            if (commandValidationError != null) return CommandResult.FromError(commandValidationError.ErrorMessage);

            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);

            await CampaignSocketUtils.SetGameMaster(Context, newGameMaster, campaign);
        
            var newGameMasterUser = await _userService.GetByDiscordUserId(newGameMaster.Id);
            campaign.GameMaster = new GameMaster { User = newGameMasterUser };
            campaign = await _campaignService.Update(campaign);
            
            await RespondAsync($"Successfully assigned {newGameMaster.Username} as the game master for this campaign.", embed: BotEmbeds.CampaignInfo(campaign));
            return CommandResult.AsSuccess();
        }

        [RequireRoleOrAdmin("Whitelisted")]
        [SlashCommand("delete", "Deletes this campaign from the server, including channels and roles.")]
        public async Task<RuntimeResult> DeleteCampaignAsync()
        {
            await DeferAsync(ephemeral: true);
            
            var commandValidationError = await _validationService.ValidateDeleteCampaignCommand(Context);
            if (commandValidationError != null) return CommandResult.FromError(commandValidationError.ErrorMessage);

            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);
            
            await CampaignSocketUtils.DeleteCampaign(Context, campaign);
            
            await _campaignService.Remove(campaign.Id);
            
            return CommandResult.AsSuccess();
        }
        
        [SlashCommand("info", "Displays all information about this campaign.")]
        public async Task<RuntimeResult> CampaignInfoAsync()
        {
            var commandValidationError = await _validationService.ValidateCampaignInfoCommand(Context);
            if (commandValidationError != null) return CommandResult.FromError(commandValidationError.ErrorMessage);
                
            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);

            await RespondAsync(embed: BotEmbeds.CampaignSessionSummary(campaign));
            return CommandResult.AsSuccess();
        }
    }
}
