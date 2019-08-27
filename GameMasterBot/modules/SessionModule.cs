using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using GameMasterBot.Services;
using GameMasterBot.Utils;
// ReSharper disable UnusedMember.Global

namespace GameMasterBot.Modules
{
    [RequireContext(ContextType.Guild)]
    [Group("session"), Name("Session"), Summary("Commands relating to managing sessions.")]
    public class SessionModule: ModuleBase<SocketCommandContext>
    {
        private readonly SessionService _service;

        public SessionModule(SessionService service) => _service = service;

        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [Command("add"), Name("add"), Alias("+"), Summary("Creates a new session for this campaign.")]
        public async Task<RuntimeResult> AddAsync(
            [Summary("The date on which the session will take place.")] string date,
            [Summary("The time at which the session will take place.")] string time)
        {
            #region Validation

            #region Date/Time

            if (!DateTime.TryParse($"{date} {time}", out var parsedDate))
                return GameMasterResult.ErrorResult("Invalid date.");

            #endregion

            #endregion

            try
            {
                var session = _service.Create(Context.Channel.Id, Context.Guild.Id, Context.Channel.Name, "AdHoc", parsedDate.ToUniversalTime()).Result;
                await ReplyAsync($"AdHoc session added for {session.Date.ToUniversalTime()} UTC.");
                return GameMasterResult.SuccessResult($"Session({date}-{time}) added successfully.");
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult(e.Message);
            }
        }

        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [Command("schedule"), Name("schedule"), Alias("++"), Summary("Schedules a new session for this session")]
        public async Task<RuntimeResult> AddAsync(
            [Summary("The date on which the session will take place.")] string date,
            [Summary("The time at which the session will take place.")] string time,
            [Summary("The schedule type for the session.")] string schedule)
        {
            #region Validation

            #region Date/Time

            if (!DateTime.TryParse($"{date} {time}", out var parsedDate))
                return GameMasterResult.ErrorResult("Invalid date.");

            #endregion

            #region Schedule

            // TODO: Use an enum for this to tidy it up
            var scheduleLower = schedule.ToLower();
            switch (scheduleLower)
            {
                case "daily":
                    schedule = "Daily";
                    break;
                case "weekly":
                    schedule = "Weekly";
                    break;
                case "biweekly":
                    schedule = "BiWeekly";
                    break;
                case "monthly":
                    schedule = "Monthly";
                    break;
                default:
                    return GameMasterResult.ErrorResult("Invalid schedule type. Choose from 'Daily', 'Weekly', 'BiWeekly', or 'Monthly'");
            }

            #endregion

            #endregion

            try
            {
                var session = _service.Create(Context.Channel.Id, Context.Guild.Id, Context.Channel.Name, schedule, parsedDate.ToUniversalTime()).Result;
                await ReplyAsync($"{schedule} session scheduled, starting on {session.Date.ToUniversalTime()} UTC.");
                return GameMasterResult.SuccessResult($"Session({session.Date}-{schedule}) scheduled successfully.");
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult(e.Message);
            }
        }


        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [Group("cancel")]
        public class CancelModule : ModuleBase<SocketCommandContext>
        {
            private readonly SessionService _service;

            public CancelModule(SessionService service) => _service = service;

            [Command, Name("cancel next"), Alias("next"), Summary("Cancels the next session for a campaign")]
            public async Task<RuntimeResult> CancelNextAsync(
                [Summary("The campaign that the session belongs to.")] string campaign = null)
            {
                #region Validation

                #region Campaign

                if (campaign == null)
                    campaign = Context.Channel.Name;

                #endregion

                #endregion

                try
                {
                    await _service.Cancel(campaign, false);
                    await ReplyAsync("Session cancel successfully.");
                    return GameMasterResult.SuccessResult("Session cancelled successfully.");
                }
                catch (Exception e)
                {
                    return GameMasterResult.ErrorResult(e.Message);
                }
            }

            [Command("day"), Name("cancel day"), Alias("date"), Summary("Cancels all sessions on a given date for a campaign")]
            public async Task<RuntimeResult> CancelDayAsync(
                [Summary("The date on which the session will take place.")] string date,
                [Summary("The campaign that the session belongs to.")] string campaign = null)
            {
                #region Validation

                #region Date

                if (!DateTime.TryParse(date, out var parsedDate))
                    return GameMasterResult.ErrorResult("Invalid date.");

                #endregion

                #region Campaign

                if (campaign == null)
                    campaign = Context.Channel.Name;

                #endregion

                #endregion

                try
                {
                    await _service.CancelForDate(campaign, parsedDate.ToUniversalTime(), false);
                    await ReplyAsync($"Sessions for {parsedDate} cancelled successfully.");
                    return GameMasterResult.SuccessResult("Sessions cancelled successfully.");
                }
                catch (Exception e)
                {
                    return GameMasterResult.ErrorResult(e.Message);
                }
            }

            [Command, Name("cancel specific"), Alias("specific"), Summary("Cancels a specific session for a campaign")]
            public async Task<RuntimeResult> CancelSpecificAsync(
                [Summary("The date on which the session will take place.")] string date,
                [Summary("The time at which the session wil take place.")] string time,
                [Summary("The campaign that the session belongs to.")] string campaign = null)
            {
                #region Validation

                #region Date

                if (!DateTime.TryParse($"{date} {time}", out var parsedDate))
                    return GameMasterResult.ErrorResult("Invalid date.");

                #endregion

                #region Campaign

                if (campaign == null)
                    campaign = Context.Channel.Name;

                #endregion

                #endregion

                try
                {
                    await _service.CancelForDateTime(campaign, parsedDate.ToUniversalTime(), false);
                    await ReplyAsync("Session cancel successfully.");
                    return GameMasterResult.SuccessResult("Session cancelled successfully.");
                }
                catch (Exception e)
                {
                    return GameMasterResult.ErrorResult(e.Message);
                }
            }
        }

        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [Command("deschedule"), Name("deschedule"), Alias("--"), Summary("Deschedule the next session for this campaign")]
        public async Task<RuntimeResult> DescheduleAsync()
        {
            try
            {
                await _service.Cancel(Context.Channel.Name, true);
                await ReplyAsync("Session de-scheduled successfully.");
                return GameMasterResult.SuccessResult("Session de-scheduled cancelled successfully.");
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult(e.Message);
            }
        }

        [Command("next"), Name("next"), Summary("Get the next session for this campaign")]
        public async Task<RuntimeResult> NextAsync()
        {
            try
            {
                var session = _service.GetNext(Context.Channel.Name);
                if (session == null)
                    await ReplyAsync("The next session for this campaign has not been scheduled yet.");
                else
                    await ReplyAsync($"The next session for this campaign will be on {session.Date.ToUniversalTime()} UTC.");
                return GameMasterResult.SuccessResult("Next session found successfully.");
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult(e.Message);
            }
        }
    }
}
