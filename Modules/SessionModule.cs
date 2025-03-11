using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord.Interactions;
using GameMasterBot.Constants;
using GameMasterBot.DTOs;
using GameMasterBot.Extensions;
using GameMasterBot.Messages;
using GameMasterBot.Models.Enums;
using GameMasterBot.Services;
using GameMasterBot.Services.Interfaces;
using GameMasterBot.Utils;

namespace GameMasterBot.Modules
{
    [RequireContext(ContextType.Guild)]
    [Group(SessionCommands.GroupName, SessionCommands.GroupDescription)]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedType.Global")]
    public class SessionModule(SessionCommandValidator validator, ISessionService sessionService, ICampaignService campaignService, IUserService userService) : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand(SessionCommands.ScheduleCommandName, SessionCommands.ScheduleCommandDescription)]
        public async Task<RuntimeResult> ScheduleSessionAsync(
            [Summary(SessionCommands.ScheduleCommandParamDateName,SessionCommands.ScheduleCommandParamDateDescription)] string date,
            [Summary(SessionCommands.ScheduleCommandParamTimeName,SessionCommands.ScheduleCommandParamTimeDescription)] string time,
            [Summary(SessionCommands.ScheduleCommandParamFrequencyName,SessionCommands.ScheduleCommandParamFrequencyDescription)]
            [Choice(SessionCommands.ScheduleCommandParamFrequencyChoiceStandaloneName, SessionCommands.ScheduleCommandParamFrequencyChoiceStandaloneValue)]
            [Choice(SessionCommands.ScheduleCommandParamFrequencyChoiceWeeklyName, SessionCommands.ScheduleCommandParamFrequencyChoiceWeeklyValue)]
            [Choice(SessionCommands.ScheduleCommandParamFrequencyChoiceFortnightlyName, SessionCommands.ScheduleCommandParamFrequencyChoiceFortnightlyValue)]
            [Choice(SessionCommands.ScheduleCommandParamFrequencyChoiceMonthlyName, SessionCommands.ScheduleCommandParamFrequencyChoiceMonthlyValue)]
            ScheduleFrequency frequency)
        {
            var scheduleSessionCommandDto = new ScheduleSessionCommandDto(date, time);
            var commandValidationError = await validator.ValidateScheduleSessionCommand(Context, scheduleSessionCommandDto);
            if (commandValidationError != null) return CommandResult.FromError(commandValidationError.ErrorMessage);
            
            var parsedDate = DateTime.ParseExact(
                $"{scheduleSessionCommandDto.Date} {scheduleSessionCommandDto.Time}", 
                SessionValidationConstants.SessionDateTimeFormat,
                CultureInfo.InvariantCulture, 
                DateTimeStyles.None);
            
            var user = await userService.GetByDiscordUserId(Context.User.Id);
            var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZoneId);
            var utcTime = TimeZoneInfo.ConvertTimeToUtc(parsedDate, tzInfo);
            
            var campaign = await campaignService.GetByTextChannelId(Context.Channel.Id);
            var session = await sessionService.Create(campaign.Id, frequency, utcTime);
            await RespondAsync(
                SessionResponseMessages.SessionSuccessfullyScheduled(), 
                embed: SessionEmbedBuilder.BuildSessionEmbed(session));
            return CommandResult.AsSuccess();
        }
        
        [SlashCommand(SessionCommands.SuggestCommandName, SessionCommands.SuggestCommandDescription)]
        public async Task<RuntimeResult> SuggestSessionAsync(
            [Summary(SessionCommands.SuggestCommandParamDateName,SessionCommands.SuggestCommandParamDateDescription)] string date,
            [Summary(SessionCommands.SuggestCommandParamTimeName,SessionCommands.SuggestCommandParamTimeDescription)] string time)
        {
            var scheduleSessionCommandDto = new ScheduleSessionCommandDto(date, time);
            var commandValidationError = await validator.ValidateSuggestSessionCommand(Context, scheduleSessionCommandDto);
            if (commandValidationError != null) return CommandResult.FromError(commandValidationError.ErrorMessage);
            
            var parsedDate = DateTime.ParseExact(
                $"{scheduleSessionCommandDto.Date} {scheduleSessionCommandDto.Time}", 
                SessionValidationConstants.SessionDateTimeFormat,
                CultureInfo.InvariantCulture, 
                DateTimeStyles.None);
            
            var user = await userService.GetByDiscordUserId(Context.User.Id);
            var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZoneId);
            var utcDateTime = TimeZoneInfo.ConvertTimeToUtc(parsedDate, tzInfo);
            
            var campaign = await campaignService.GetByTextChannelId(Context.Channel.Id);

            await RespondAsync(
                SessionResponseMessages.SessionSuccessfullySuggested(), 
                embed: SessionEmbedBuilder.BuildSuggestionEmbed(campaign, utcDateTime));
            return CommandResult.AsSuccess();
        }

        [SlashCommand(SessionCommands.ViewNextCommandName, SessionCommands.ViewNextCommandDescription)]
        public async Task<RuntimeResult> ViewNextSessionAsync()
        {
            var commandValidationError = await validator.ValidateViewNextSessionCommand(Context);
            if (commandValidationError != null) return CommandResult.FromError(commandValidationError.ErrorMessage);
            
            var campaign = await campaignService.GetByTextChannelId(Context.Channel.Id);
            var sessions = await sessionService.GetAllUpcomingByCampaignId(campaign.Id);

            await RespondAsync(
                SessionResponseMessages.NextSessionScheduled(),
                embed: SessionEmbedBuilder.BuildSessionEmbed(sessions.First()),
                ephemeral: true);
            return CommandResult.AsSuccess();
        }
        
        [SlashCommand(SessionCommands.ViewUpcomingCommandName, SessionCommands.ViewUpcomingCommandDescription)]
        public async Task<RuntimeResult> ViewUpcomingSessionsAsync()
        {
            var commandValidationError = await validator.ValidateViewUpcomingSessionsCommand(Context);
            if (commandValidationError != null) return CommandResult.FromError(commandValidationError.ErrorMessage);
            
            var campaign = await campaignService.GetByTextChannelId(Context.Channel.Id);
            var sessions = await sessionService.GetAllUpcomingByCampaignId(campaign.Id);
            var user = await userService.GetByDiscordUserId(Context.User.Id);

            await RespondAsync(
                embed: SessionEmbedBuilder.BuildSessionListEmbed(user, sessions),
                ephemeral: true);
            return CommandResult.AsSuccess();
        }

        [SlashCommand(SessionCommands.CancelNextCommandName, SessionCommands.CancelNextCommandDescription)]
        public async Task<RuntimeResult> CancelNextSessionAsync()
        {
            var commandValidationError = await validator.ValidateCancelNextSessionCommand(Context);
            if (commandValidationError != null) return CommandResult.FromError(commandValidationError.ErrorMessage);
            
            var campaign = await campaignService.GetByTextChannelId(Context.Channel.Id);
            await sessionService.CancelNextByCampaignId(campaign.Id);

            var upcomingSessions = await sessionService.GetAllUpcomingByCampaignId(campaign.Id);

            if (upcomingSessions.Count == 0)
                await RespondAsync(
                    $"{SessionResponseMessages.NextSessionCancelled()} {SessionResponseMessages.NoMoreUpcomingSessions()}");
            else
                await RespondAsync(
                    $"{SessionResponseMessages.NextSessionCancelled()} {SessionResponseMessages.FollowingSessionDetails()}",
                    embed: SessionEmbedBuilder.BuildSessionEmbed(upcomingSessions.First()));
            return CommandResult.AsSuccess();
        }

        [SlashCommand(SessionCommands.CancelCommandName, SessionCommands.CancelCommandDescription)]
        public async Task<RuntimeResult> CancelSessionAsync(
            [Summary(SessionCommands.CancelCommandParamDateName,SessionCommands.CancelCommandParamDateDescription)] string date,
            [Summary(SessionCommands.CancelCommandParamTimeName,SessionCommands.CancelCommandParamTimeDescription)] string time)
        {
            var cancelSessionDto = new CancelSessionDto(date, time);
            
            var commandValidationError = await validator.ValidateCancelSessionCommand(Context, cancelSessionDto);
            if (commandValidationError != null) return CommandResult.FromError(commandValidationError.ErrorMessage);
            
            var parsedDate = DateTime.ParseExact(
                $"{date} {time}", 
                SessionValidationConstants.SessionDateTimeFormat,
                CultureInfo.InvariantCulture, 
                DateTimeStyles.None);
            var user = await userService.GetByDiscordUserId(Context.User.Id);
            var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZoneId);
            var utcTime = TimeZoneInfo.ConvertTimeToUtc(parsedDate, tzInfo);
            
            var campaign = await campaignService.GetByTextChannelId(Context.Channel.Id);
            await sessionService.CancelAllByCampaignIdAndTimestamp(campaign.Id, utcTime);
            
            var upcomingSessions = await sessionService.GetAllUpcomingByCampaignId(campaign.Id);
            if (upcomingSessions.Count == 0)
                await RespondAsync(
                    $"{SessionResponseMessages.SessionCancelled()} {SessionResponseMessages.NoMoreUpcomingSessions()}");
            else
                await RespondAsync(
                    $"{SessionResponseMessages.SessionCancelled()} {SessionResponseMessages.FollowingSessionDetails()}",
                    embed: SessionEmbedBuilder.BuildSessionEmbed(upcomingSessions.First()));
            
            return CommandResult.AsSuccess();
        }

        [SlashCommand(SessionCommands.CancelRecurringCommandName, SessionCommands.CancelRecurringCommandDescription)]
        public async Task<RuntimeResult> CancelRecurringSessionAsync(
            [Summary(SessionCommands.CancelRecurringCommandParamDateName,SessionCommands.CancelRecurringCommandParamDateDescription)] string date,
            [Summary(SessionCommands.CancelRecurringCommandParamTimeName,SessionCommands.CancelRecurringCommandParamTimeDescription)] string time)
        {
            var cancelSessionDto = new CancelSessionDto(date, time);
            
            var commandValidationError = await validator.ValidateCancelRecurringSessionCommand(Context, cancelSessionDto);
            if (commandValidationError != null) return CommandResult.FromError(commandValidationError.ErrorMessage);
            
            var parsedDate = DateTime.ParseExact(
                $"{date} {time}", 
                SessionValidationConstants.SessionDateTimeFormat,
                CultureInfo.InvariantCulture, 
                DateTimeStyles.None);
            var user = await userService.GetByDiscordUserId(Context.User.Id);
            var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZoneId);
            var utcTime = TimeZoneInfo.ConvertTimeToUtc(parsedDate, tzInfo);
            
            var campaign = await campaignService.GetByTextChannelId(Context.Channel.Id);
            
            await sessionService.CancelAllRecurringByCampaignIdAndTimestamp(campaign.Id, utcTime);

            var upcomingSessions = await sessionService.GetAllUpcomingByCampaignId(campaign.Id);
            if (upcomingSessions.Count == 0)
                await RespondAsync(
                    $"{SessionResponseMessages.RecurringSessionCancelled()} {SessionResponseMessages.NoMoreUpcomingSessions()}");
            else
                await RespondAsync(
                    $"{SessionResponseMessages.RecurringSessionCancelled()} {SessionResponseMessages.FollowingSessionDetails()}",
                    embed: SessionEmbedBuilder.BuildSessionEmbed(upcomingSessions.First()));
            return CommandResult.AsSuccess();
        }
        
        [SlashCommand(SessionCommands.CancelAllCommandName, SessionCommands.CancelAllCommandDescription)]
        public async Task<RuntimeResult> CancelAllSessionsAsync()
        {
            var commandValidationError = await validator.ValidateCancelAllSessionsCommand(Context);
            if (commandValidationError != null) return CommandResult.FromError(commandValidationError.ErrorMessage);
            
            var campaign = await campaignService.GetByTextChannelId(Context.Channel.Id);
            await sessionService.CancelAllByCampaignId(campaign.Id);

            await RespondAsync(SessionResponseMessages.AllSessionsCancelled());
            return CommandResult.AsSuccess();
        }
    }
}
