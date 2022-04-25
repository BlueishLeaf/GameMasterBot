using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord.Interactions;
using GameMasterBot.Constants;
using GameMasterBot.DTOs;
using GameMasterBot.Extensions;
using GameMasterBot.Messages;
using GameMasterBot.Services.Interfaces;

namespace GameMasterBot.Services;

public class SessionCommandValidator
{
    private readonly ICampaignService _campaignService;
    private readonly ISessionService _sessionService;
    private readonly IUserService _userService;

    public SessionCommandValidator(ICampaignService campaignService, ISessionService sessionService, IUserService userService)
    {
        _campaignService = campaignService;
        _sessionService = sessionService;
        _userService = userService;
    }

    public async Task<CommandValidationError> ValidateScheduleSessionCommand(SocketInteractionContext context, ScheduleSessionCommandDto scheduleSessionCommandDto)
    {
        var campaign = await _campaignService.GetByTextChannelId(context.Channel.Id);
        if (campaign == null)
            return CommonValidationMessages.NotInCampaignChannel();

        var commandIssuer = context.Guild.GetUser(context.User.Id);
        if (campaign.GameMaster.User.DiscordId != context.User.Id && !commandIssuer.GuildPermissions.Administrator)
            return CommonValidationMessages.NotGameMasterOrAdmin();

        if (!DateTime.TryParseExact($"{scheduleSessionCommandDto.Date} {scheduleSessionCommandDto.Time}", SessionValidationConstants.SessionDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            return SessionValidationMessages.InvalidDateTime();

        var user = await _userService.GetByDiscordUserId(context.User.Id);
        var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZoneId);
        var utcTime = TimeZoneInfo.ConvertTimeToUtc(parsedDate, tzInfo);
        return utcTime <= DateTime.UtcNow ?
            SessionValidationMessages.DateIsInPast() :
            null;
    }

    public async Task<CommandValidationError> ValidateViewNextSessionCommand(SocketInteractionContext context)
    {
        var campaign = await _campaignService.GetByTextChannelId(context.Channel.Id);
        if (campaign == null)
            return CommonValidationMessages.NotInCampaignChannel();

        var sessions = await _sessionService.GetAllUpcomingByCampaignId(campaign.Id);
        return !sessions.Any() ?
            SessionValidationMessages.NoUpcomingSessions() :
            null;
    }

    public async Task<CommandValidationError> ValidateViewUpcomingSessionsCommand(SocketInteractionContext context)
    {
        var campaign = await _campaignService.GetByTextChannelId(context.Channel.Id);
        if (campaign == null)
            return CommonValidationMessages.NotInCampaignChannel();

        var sessions = await _sessionService.GetAllUpcomingByCampaignId(campaign.Id);
        return !sessions.Any() ?
            SessionValidationMessages.NoUpcomingSessions() :
            null;
    }
    
    public async Task<CommandValidationError> ValidateCancelSessionCommand(SocketInteractionContext context, CancelSessionDto cancelSessionDto)
    {
        var campaign = await _campaignService.GetByTextChannelId(context.Channel.Id);
        if (campaign == null)
            return CommonValidationMessages.NotInCampaignChannel();

        var commandIssuer = context.Guild.GetUser(context.User.Id);
        if (campaign.GameMaster.User.DiscordId != context.User.Id && !commandIssuer.GuildPermissions.Administrator)
            return CommonValidationMessages.NotGameMasterOrAdmin();
        
        if (!DateTime.TryParseExact($"{cancelSessionDto.Date} {cancelSessionDto.Time}", SessionValidationConstants.SessionDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            return SessionValidationMessages.InvalidDateTime();
        
        var user = await _userService.GetByDiscordUserId(context.User.Id);
        var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZoneId);
        var utcDateTime = TimeZoneInfo.ConvertTimeToUtc(parsedDate, tzInfo);
        
        var sessions = await _sessionService.GetAllByCampaignIdAndTimestamp(campaign.Id, utcDateTime);
        return !sessions.Any() ?
            SessionValidationMessages.NoSessionsForTimestamp() :
            null;
    }

    public async Task<CommandValidationError> ValidateCancelNextSessionCommand(SocketInteractionContext context)
    {
        var campaign = await _campaignService.GetByTextChannelId(context.Channel.Id);
        if (campaign == null)
            return CommonValidationMessages.NotInCampaignChannel();

        var commandIssuer = context.Guild.GetUser(context.User.Id);
        if (campaign.GameMaster.User.DiscordId != context.User.Id && !commandIssuer.GuildPermissions.Administrator)
            return CommonValidationMessages.NotGameMasterOrAdmin();

        var sessions = await _sessionService.GetAllUpcomingByCampaignId(campaign.Id);
        return !sessions.Any() ?
            SessionValidationMessages.NoUpcomingSessions() :
            null;
    }

    public async Task<CommandValidationError> ValidateCancelRecurringSessionCommand(SocketInteractionContext context, CancelSessionDto cancelSessionDto)
    {
        var campaign = await _campaignService.GetByTextChannelId(context.Channel.Id);
        if (campaign == null)
            return CommonValidationMessages.NotInCampaignChannel();

        var commandIssuer = context.Guild.GetUser(context.User.Id);
        if (campaign.GameMaster.User.DiscordId != context.User.Id && !commandIssuer.GuildPermissions.Administrator)
            return CommonValidationMessages.NotGameMasterOrAdmin();
        
        if (!DateTime.TryParseExact($"{cancelSessionDto.Date} {cancelSessionDto.Time}", SessionValidationConstants.SessionDateTimeFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            return SessionValidationMessages.InvalidDateTime();
        
        var user = await _userService.GetByDiscordUserId(context.User.Id);
        var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZoneId);
        var utcDateTime = TimeZoneInfo.ConvertTimeToUtc(parsedDate, tzInfo);

        var existingRecurringSchedule = await _sessionService.GetAllRecurringByCampaignIdAndTimestamp(campaign.Id, utcDateTime);
        return !existingRecurringSchedule.Any() ?
            SessionValidationMessages.NoRecurringSessionsForTimestamp() :
            null;
    }
    
    public async Task<CommandValidationError> ValidateCancelAllSessionsCommand(SocketInteractionContext context)
    {
        var campaign = await _campaignService.GetByTextChannelId(context.Channel.Id);
        if (campaign == null)
            return CommonValidationMessages.NotInCampaignChannel();

        var commandIssuer = context.Guild.GetUser(context.User.Id);
        if (campaign.GameMaster.User.DiscordId != context.User.Id && !commandIssuer.GuildPermissions.Administrator)
            return CommonValidationMessages.NotGameMasterOrAdmin();

        var sessions = await _sessionService.GetAllUpcomingByCampaignId(campaign.Id);
        return !sessions.Any() ?
            SessionValidationMessages.NoUpcomingSessions() :
            null;
    }
}