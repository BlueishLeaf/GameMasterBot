﻿using System;
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
// Modules and their methods are picked up by the handler but not recognised by Rider
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace GameMasterBot.Modules
{
    [RequireContext(ContextType.Guild)]
    [Group(SessionCommands.GroupName, SessionCommands.GroupDescription)]
    public class SessionModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly SessionCommandValidator _validator;
        private readonly ISessionService _sessionService;
        private readonly ICampaignService _campaignService;
        private readonly IUserService _userService;

        public SessionModule(SessionCommandValidator validator, ISessionService sessionService, ICampaignService campaignService, IUserService userService)
        {
            _validator = validator;
            _sessionService = sessionService;
            _campaignService = campaignService;
            _userService = userService;
        }

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
            var scheduleSessionCommandDto = new ScheduleSessionCommandDto(date, time, frequency);
            var commandValidationError = await _validator.ValidateScheduleSessionCommand(Context, scheduleSessionCommandDto);
            if (commandValidationError != null) return CommandResult.FromError(commandValidationError.ErrorMessage);
            
            var parsedDate = DateTime.ParseExact(
                $"{scheduleSessionCommandDto.Date} {scheduleSessionCommandDto.Time}", 
                SessionValidationConstants.SessionDateTimeFormat,
                CultureInfo.InvariantCulture, 
                DateTimeStyles.None);
            
            var user = await _userService.GetByDiscordUserId(Context.User.Id);
            var tzInfo = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZoneId);
            var utcTime = TimeZoneInfo.ConvertTimeToUtc(parsedDate, tzInfo);
            
            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);
            var session = await _sessionService.Create(campaign.Id, frequency, utcTime);
            await RespondAsync(
                SessionResponseMessages.SessionSuccessfullyScheduled(), 
                embed: SessionEmbedBuilder.BuildSessionEmbed(session));
            return CommandResult.AsSuccess();
        }
        
        [SlashCommand(SessionCommands.ViewNextCommandName, SessionCommands.ViewNextCommandDescription)]
        public async Task<RuntimeResult> ViewNextSessionAsync()
        {
            var commandValidationError = await _validator.ValidateViewNextSessionCommand(Context);
            if (commandValidationError != null) return CommandResult.FromError(commandValidationError.ErrorMessage);
            
            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);
            var sessions = await _sessionService.GetUpcomingByCampaignId(campaign.Id);

            await RespondAsync(
                SessionResponseMessages.NextSessionScheduled(campaign.Name),
                embed: SessionEmbedBuilder.BuildSessionEmbed(sessions.First()),
                ephemeral: true);
            return CommandResult.AsSuccess();
        }
        
        [SlashCommand(SessionCommands.ViewUpcomingCommandName, SessionCommands.ViewUpcomingCommandDescription)]
        public async Task<RuntimeResult> ViewUpcomingSessionsAsync()
        {
            var commandValidationError = await _validator.ValidateViewUpcomingSessionsCommand(Context);
            if (commandValidationError != null) return CommandResult.FromError(commandValidationError.ErrorMessage);
            
            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);
            var sessions = await _sessionService.GetUpcomingByCampaignId(campaign.Id);
            var user = await _userService.GetByDiscordUserId(Context.User.Id);

            await RespondAsync(
                embed: SessionEmbedBuilder.BuildSessionListEmbed(user, sessions),
                ephemeral: true);
            return CommandResult.AsSuccess();
        }

        [SlashCommand(SessionCommands.CancelNextCommandName, SessionCommands.CancelNextCommandDescription)]
        public async Task<RuntimeResult> CancelNextSessionAsync()
        {
            var commandValidationError = await _validator.ValidateCancelNextSessionCommand(Context);
            if (commandValidationError != null) return CommandResult.FromError(commandValidationError.ErrorMessage);
            
            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);
            await _sessionService.CancelNext(campaign.Id);

            var upcomingSessions = await _sessionService.GetUpcomingByCampaignId(campaign.Id);
            if (upcomingSessions.Count == 0)
                await RespondAsync(
                    $"{SessionResponseMessages.NextSessionCancelled()} {SessionResponseMessages.NoMoreUpcomingSessions(campaign.Name)}");
            else
                await RespondAsync(
                    $"{SessionResponseMessages.NextSessionCancelled()} {SessionResponseMessages.FollowingSessionDetails(campaign.Name)}",
                    embed: SessionEmbedBuilder.BuildSessionEmbed(upcomingSessions.First()));
            return CommandResult.AsSuccess();
        }
        
        [SlashCommand(SessionCommands.CancelRecurringCommandName, SessionCommands.CancelRecurringCommandDescription)]
        public async Task<RuntimeResult> CancelRecurringSessionAsync()
        {
            var commandValidationError = await _validator.ValidateCancelRecurringSessionCommand(Context);
            if (commandValidationError != null) return CommandResult.FromError(commandValidationError.ErrorMessage);
            
            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);
            var existingRecurringSchedule = await _sessionService.GetRecurringByCampaignId(campaign.Id);
            
            await _sessionService.CancelRecurringById(existingRecurringSchedule.Id);

            var upcomingSessions = await _sessionService.GetUpcomingByCampaignId(campaign.Id);
            if (upcomingSessions.Count == 0)
                await RespondAsync(
                    $"{SessionResponseMessages.RecurringSessionCancelled()} {SessionResponseMessages.NoMoreUpcomingSessions(campaign.Name)}");
            else
                await RespondAsync(
                    $"{SessionResponseMessages.RecurringSessionCancelled()} {SessionResponseMessages.FollowingSessionDetails(campaign.Name)}",
                    embed: SessionEmbedBuilder.BuildSessionEmbed(upcomingSessions.First()));
            return CommandResult.AsSuccess();
        }
    }
}
