using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using GameMasterBot.Constants;
using GameMasterBot.DTOs;
using GameMasterBot.Extensions;
using GameMasterBot.Messages;
using GameMasterBot.Services.Interfaces;

namespace GameMasterBot.Services;

public class CampaignCommandValidator
{
    private readonly ICampaignService _campaignService;

    public CampaignCommandValidator(ICampaignService campaignService) => _campaignService = campaignService;

    public async Task<CommandValidationError> ValidateCreateCampaignCommand(SocketInteractionContext context, CreateCampaignCommandDto createCampaignCommandDto)
    {
        if (createCampaignCommandDto.CampaignName.Length > CampaignValidationConstants.NameMaxLength)
            return CampaignValidationMessages.InvalidNameLength();

        var channelRegex = new Regex(CampaignValidationConstants.NameRegexPattern);
        if (!channelRegex.IsMatch(createCampaignCommandDto.CampaignName))
            return CampaignValidationMessages.InvalidNamePattern();

        if (createCampaignCommandDto.GameSystem.Length > CampaignValidationConstants.NameMaxLength)
            return CampaignValidationMessages.InvalidSystemLength();

        if (!channelRegex.IsMatch(createCampaignCommandDto.GameSystem))
            return CampaignValidationMessages.InvalidSystemPattern();

        var guildCampaigns = await _campaignService.GetAllByGuildId(context.Guild.Id);
        return guildCampaigns.Any(c => c.Name == createCampaignCommandDto.CampaignName)
            ? CampaignValidationMessages.CampaignAlreadyExists()
            : null;
    }
    
    public async Task<CommandValidationError> ValidateAddPlayerCommand(SocketInteractionContext context, IUser newPlayer)
    {
        var campaign = await _campaignService.GetByTextChannelId(context.Channel.Id);
        if (campaign == null)
            return CommonValidationMessages.NotInCampaignChannel();

        if (newPlayer.Id == campaign.GameMaster.User.DiscordId)
            return CampaignValidationMessages.CannotAddGameMaster(newPlayer.Id);

        var commandIssuer = context.Guild.GetUser(context.User.Id);
        if (campaign.GameMaster.User.DiscordId != context.User.Id && !commandIssuer.GuildPermissions.Administrator)
            return CommonValidationMessages.NotGameMasterOrAdmin();
            
        var campaignRole = context.Guild.Roles.FirstOrDefault(role => role.Id == campaign.PlayerRoleId);
        if (campaignRole == null)
            return CampaignValidationMessages.NoPlayerRole();

        var foundPlayer = campaign.Players.FirstOrDefault(cu => cu.User.DiscordId == newPlayer.Id);
        return foundPlayer != null ?
            CampaignValidationMessages.CannotAddExistingPlayer(newPlayer.Id) :
            null;
    }
    
    public async Task<CommandValidationError> ValidateRemovePlayerCommand(SocketInteractionContext context, IUser playerToRemove)
    {
        var campaign = await _campaignService.GetByTextChannelId(context.Channel.Id);
        if (campaign == null)
            return CommonValidationMessages.NotInCampaignChannel();

        var commandIssuer = context.Guild.GetUser(context.User.Id);
        if (campaign.GameMaster.User.DiscordId != context.User.Id && !commandIssuer.GuildPermissions.Administrator)
            return CommonValidationMessages.NotGameMasterOrAdmin();

        var foundPlayer = campaign.Players.FirstOrDefault(cu => cu.User.DiscordId == playerToRemove.Id);
        if (foundPlayer == null)
            return CampaignValidationMessages.CannotRemoveNonPlayer(playerToRemove.Id);
            
        var campaignRole = context.Guild.Roles.FirstOrDefault(role => role.Id == campaign.PlayerRoleId);
        return campaignRole == null ?
            CampaignValidationMessages.NoPlayerRole() :
            null;
    }
    
    public async Task<CommandValidationError> ValidateSetUrlCommand(SocketInteractionContext context, string url)
    {
        var campaign = await _campaignService.GetByTextChannelId(context.Channel.Id);
        if (campaign == null)
            return CommonValidationMessages.NotInCampaignChannel();
            
        var commandIssuer = context.Guild.GetUser(context.User.Id);
        if (campaign.GameMaster.User.DiscordId != context.User.Id && !commandIssuer.GuildPermissions.Administrator)
            return CommonValidationMessages.NotGameMasterOrAdmin();

        return !Uri.IsWellFormedUriString(url, UriKind.Absolute) ? CampaignValidationMessages.InvalidUrl() : null;
    }
    
    public async Task<CommandValidationError> ValidateSetGameMaster(SocketInteractionContext context, IUser newGameMaster)
    {
        var campaign = await _campaignService.GetByTextChannelId(context.Channel.Id);
        if (campaign == null)
            return CommonValidationMessages.NotInCampaignChannel();
            
        var commandIssuer = context.Guild.GetUser(context.User.Id);
        if (campaign.GameMaster.User.DiscordId != context.User.Id && !commandIssuer.GuildPermissions.Administrator)
            return CommonValidationMessages.NotGameMasterOrAdmin();

        if (campaign.GameMaster.User.DiscordId == newGameMaster.Id)
            return CampaignValidationMessages.CannotSetCurrentGameMaster(newGameMaster.Id);

        if (context.Guild.Roles.All(role => role.Id != campaign.GameMasterRoleId))
            return CampaignValidationMessages.NoGameMasterRole();

        return context.Guild.Roles.All(role => role.Id != campaign.PlayerRoleId) ?
            CampaignValidationMessages.NoPlayerRole() :
            null;
    }
    
    public async Task<CommandValidationError> ValidateDeleteCampaignCommand(SocketInteractionContext context)
    {
        var campaign = await _campaignService.GetByTextChannelId(context.Channel.Id);
        if (campaign == null)
            return CommonValidationMessages.NotInCampaignChannel();

        var commandIssuer = context.Guild.GetUser(context.User.Id);
        var notGmOrAdmin =
            campaign.GameMaster.User.DiscordId != context.User.Id &&
            !commandIssuer.GuildPermissions.Administrator;
        return notGmOrAdmin
            ? CommonValidationMessages.NotGameMasterOrAdmin()
            : null;
    }
    
    public async Task<CommandValidationError> ValidateCampaignInfoCommand(SocketInteractionContext context)
    {
        var campaign = await _campaignService.GetByTextChannelId(context.Channel.Id);
        return campaign == null ?
            CommonValidationMessages.NotInCampaignChannel() :
            null;
    }
}