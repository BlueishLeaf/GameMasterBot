using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using GameMasterBot.Services;
using GameMasterBot.Utils;
// ReSharper disable UnusedMember.Global

namespace GameMasterBot.Modules
{
    [RequireContext(ContextType.Guild)]
    [Group("session"), Name("Session")]
    public class SessionModule: ModuleBase<SocketCommandContext>
    {
        private readonly SessionService _service;

        public SessionModule(SessionService service) => _service = service;

        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [Command("add"), Alias("+")]
        [Summary("Creates a new session for this campaign.")]
        public async Task<RuntimeResult> AddAsync(
            [Summary("The date on which the session will take place.")] string date,
            [Summary("The time at which the session will take place.")] string time,
            [Summary("The schedule type for the session.")] string campaign = null)
        {
            #region Validation

            #region Campaign

            string campaignId;
            ulong channelId;
            if (campaign == null)
            {
                campaignId = Context.Channel.Name;
                campaign = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(campaignId.Replace('-', ' '));
                var campaignTextChannel = Context.Guild.TextChannels.FirstOrDefault(chan => chan.Name == campaignId);
                if (campaignTextChannel == null)
                    return GameMasterResult.ErrorResult("Campaign does not exist on this server.");
                channelId = campaignTextChannel.Id;
            }
            else
            {
                campaignId = campaign.Replace(' ', '-').ToLower();
                campaign = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(campaign.Replace('-', ' ').ToLower());
                var campaignTextChannel = Context.Guild.TextChannels.FirstOrDefault(chan => chan.Name == campaignId);
                if (campaignTextChannel == null)
                    return GameMasterResult.ErrorResult("Campaign does not exist on this server.");
                channelId = campaignTextChannel.Id;
            }

            #endregion

            #region Date/Time

            if (!DateTime.TryParse($"{date} {time}", out var parsedDate))
                return GameMasterResult.ErrorResult("Invalid date.");

            #endregion

            #endregion

            try
            {
                var session = _service.Create(channelId, Context.Guild.Id, Context.Guild.Name, campaignId, campaign, "AdHoc", parsedDate.ToUniversalTime()).Result;
                await ReplyAsync(embed: EmbedUtils.SessionInfo($"Session Added for {session.CampaignName}", session));
                return GameMasterResult.SuccessResult();
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult(e.Message);
            }
        }

        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [Command("schedule"), Alias("++")]
        [Summary("Schedules a new session for a campaign")]
        public async Task<RuntimeResult> ScheduleAsync(
            [Summary("The date on which the session will take place.")] string date,
            [Summary("The time at which the session will take place.")] string time,
            [Summary("The schedule type for the session.")] string schedule,
            [Summary("The schedule type for the session.")] string campaign = null)
        {
            #region Validation

            #region Campaign

            string campaignId;
            ulong channelId;
            if (campaign == null)
            {
                campaignId = Context.Channel.Name;
                campaign = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(campaignId.Replace('-', ' '));
                var campaignTextChannel = Context.Guild.TextChannels.FirstOrDefault(chan => chan.Name == campaignId);
                if (campaignTextChannel == null)
                    return GameMasterResult.ErrorResult("Campaign does not exist on this server.");
                channelId = campaignTextChannel.Id;
            }
            else
            {
                campaignId = campaign.Replace(' ', '-').ToLower();
                campaign = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(campaign.Replace('-', ' ').ToLower());
                var campaignTextChannel = Context.Guild.TextChannels.FirstOrDefault(chan => chan.Name == campaignId);
                if (campaignTextChannel == null)
                    return GameMasterResult.ErrorResult("Campaign does not exist on this server.");
                channelId = campaignTextChannel.Id;
            }

            #endregion

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
                var session = _service.Create(channelId, Context.Guild.Id, Context.Guild.Name, campaignId, campaign , schedule, parsedDate.ToUniversalTime()).Result;
                await ReplyAsync(embed: EmbedUtils.SessionInfo($"Session Scheduled for {session.CampaignName}", session));
                return GameMasterResult.SuccessResult();
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult(e.Message);
            }
        }

        [Command("next"), Alias("closest")]
        [Summary("Get the next session for this campaign.")]
        public async Task<RuntimeResult> NextAsync(
            [Summary("The name of the campaign.")] string campaign = null)
        {
            #region Validation

            #region Campaign

            string campaignId;
            if (campaign == null)
            {
                campaignId = Context.Channel.Name;
                var campaignTextChannel = Context.Guild.TextChannels.FirstOrDefault(chan => chan.Name == campaignId);
                if (campaignTextChannel == null)
                    return GameMasterResult.ErrorResult("Campaign does not exist on this server.");
            }
            else
            {
                campaignId = campaign.Replace(' ', '-').ToLower();
                var campaignTextChannel = Context.Guild.TextChannels.FirstOrDefault(chan => chan.Name == campaignId);
                if (campaignTextChannel == null)
                    return GameMasterResult.ErrorResult("Campaign does not exist on this server.");
            }

            #endregion

            #endregion

            try
            {
                var session = _service.GetUpcoming(Context.Guild.Id, campaignId).FirstOrDefault();
                if (session == null)
                    return GameMasterResult.ErrorResult("This campaign does not exist or the next session for this campaign has not been scheduled yet.");

                await ReplyAsync(embed: EmbedUtils.SessionInfo($"Next Session for {session.CampaignName}", session));
                return GameMasterResult.SuccessResult();
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult(e.Message);
            }
        }

        [Command("upcoming"), Alias("soon")]
        [Summary("Get all the upcoming sessions for this campaign.")]
        public async Task<RuntimeResult> UpcomingAsync(
            [Summary("The name of the campaign.")] string campaign = null)
        {
            #region Validation

            #region Campaign

            string campaignId;
            if (campaign == null)
            {
                campaignId = Context.Channel.Name;
                campaign = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(campaignId.Replace('-', ' '));
                var campaignTextChannel = Context.Guild.TextChannels.FirstOrDefault(chan => chan.Name == campaignId);
                if (campaignTextChannel == null)
                    return GameMasterResult.ErrorResult("Campaign does not exist on this server.");
            }
            else
            {
                campaignId = campaign.Replace(' ', '-').ToLower();
                campaign = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(campaign.Replace('-', ' ').ToLower());
                var campaignTextChannel = Context.Guild.TextChannels.FirstOrDefault(chan => chan.Name == campaignId);
                if (campaignTextChannel == null)
                    return GameMasterResult.ErrorResult("Campaign does not exist on this server.");
            }

            #endregion

            #endregion

            try
            {
                var sessions = _service.GetUpcoming(Context.Guild.Id, campaignId).ToList();
                if (!sessions.Any())
                    return GameMasterResult.ErrorResult("This campaign does not exist or the next session for this campaign has not been scheduled yet.");

                await ReplyAsync(embed: EmbedUtils.SessionList($"Upcoming Sessions for {campaign}", sessions));
                return GameMasterResult.SuccessResult();
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult(e.Message);
            }
        }

        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [Command("cancel next"), Alias("cancel upcoming")]
        [Summary("Cancels the next session for a campaign")]
        public async Task<RuntimeResult> CancelNextAsync(
            [Summary("The campaign that the session belongs to.")] string campaign = null)
        {
            #region Validation

            #region Campaign

            string campaignId;
            if (campaign == null)
            {
                campaignId = Context.Channel.Name;
                var campaignTextChannel = Context.Guild.TextChannels.FirstOrDefault(chan => chan.Name == campaignId);
                if (campaignTextChannel == null)
                    return GameMasterResult.ErrorResult("Campaign does not exist on this server.");
            }
            else
            {
                campaignId = campaign.Replace(' ', '-').ToLower();
                var campaignTextChannel = Context.Guild.TextChannels.FirstOrDefault(chan => chan.Name == campaignId);
                if (campaignTextChannel == null)
                    return GameMasterResult.ErrorResult("Campaign does not exist on this server.");
            }

            #endregion

            #endregion

            try
            {
                await _service.CancelNext(Context.Guild.Id, campaignId);
                await ReplyAsync("Next session cancelled successfully.");
                return GameMasterResult.SuccessResult();
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult(e.Message);
            }
        }

        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [Command("cancel day"), Alias("cancel date")]
        [Summary("Cancels all sessions on a given date for a campaign")]
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

            string campaignId;
            if (campaign == null)
            {
                campaignId = Context.Channel.Name;
                var campaignTextChannel = Context.Guild.TextChannels.FirstOrDefault(chan => chan.Name == campaignId);
                if (campaignTextChannel == null)
                    return GameMasterResult.ErrorResult("Campaign does not exist on this server.");
            }
            else
            {
                campaignId = campaign.Replace(' ', '-').ToLower();
                var campaignTextChannel = Context.Guild.TextChannels.FirstOrDefault(chan => chan.Name == campaignId);
                if (campaignTextChannel == null)
                    return GameMasterResult.ErrorResult("Campaign does not exist on this server.");
            }

            #endregion

            #endregion

            try
            {
                await _service.CancelForDay(Context.Guild.Id, campaignId, parsedDate.ToUniversalTime());
                await ReplyAsync($"All sessions on {parsedDate.ToShortDateString()} cancelled successfully.");
                return GameMasterResult.SuccessResult();
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult(e.Message);
            }
        }

        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [Command("cancel period"), Alias("cancel range")]
        [Summary("Cancels sessions on a range of dates for a campaign.")]
        public async Task<RuntimeResult> CancelPeriodAsync(
            [Summary("The start of the date range.")] string after,
            [Summary("The end of the date range.")] string before,
            [Summary("The campaign that the session belongs to.")] string campaign = null)
        {
            #region Validation

            #region Date

            if (!DateTime.TryParse(after, out var parsedAfterDate))
                return GameMasterResult.ErrorResult("Invalid 'After' date.");

            if (!DateTime.TryParse(before, out var parsedBeforeDate))
                return GameMasterResult.ErrorResult("Invalid 'Before' date.");

            #endregion

            #region Campaign

            string campaignId;
            if (campaign == null)
            {
                campaignId = Context.Channel.Name;
                var campaignTextChannel = Context.Guild.TextChannels.FirstOrDefault(chan => chan.Name == campaignId);
                if (campaignTextChannel == null)
                    return GameMasterResult.ErrorResult("Campaign does not exist on this server.");
            }
            else
            {
                campaignId = campaign.Replace(' ', '-').ToLower();
                var campaignTextChannel = Context.Guild.TextChannels.FirstOrDefault(chan => chan.Name == campaignId);
                if (campaignTextChannel == null)
                    return GameMasterResult.ErrorResult("Campaign does not exist on this server.");
            }

            #endregion

            #endregion

            try
            {
                await _service.CancelForPeriod(Context.Guild.Id, campaignId, parsedAfterDate.ToUniversalTime(), parsedBeforeDate.ToUniversalTime());
                await ReplyAsync($"All sessions from {parsedAfterDate.ToShortDateString()} to {parsedBeforeDate.ToShortDateString()} cancelled successfully.");
                return GameMasterResult.SuccessResult();
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult(e.Message);
            }
        }

        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [Command("cancel exact"), Alias("cancel specific")]
        [Summary("Cancels a specific session for a campaign")]
        public async Task<RuntimeResult> CancelDayTimeAsync(
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

            string campaignId;
            if (campaign == null)
            {
                campaignId = Context.Channel.Name;
                var campaignTextChannel = Context.Guild.TextChannels.FirstOrDefault(chan => chan.Name == campaignId);
                if (campaignTextChannel == null)
                    return GameMasterResult.ErrorResult("Campaign does not exist on this server.");
            }
            else
            {
                campaignId = campaign.Replace(' ', '-').ToLower();
                var campaignTextChannel = Context.Guild.TextChannels.FirstOrDefault(chan => chan.Name == campaignId);
                if (campaignTextChannel == null)
                    return GameMasterResult.ErrorResult("Campaign does not exist on this server.");
            }

            #endregion

            #endregion

            try
            {
                await _service.CancelForDayTime(Context.Guild.Id, campaignId, parsedDate.ToUniversalTime());
                await ReplyAsync($"Session on {parsedDate.ToShortDateString()} at {parsedDate.ToShortTimeString()} cancelled successfully.");
                return GameMasterResult.SuccessResult();
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult(e.Message);
            }
        }

        [RequireUserPermission(ChannelPermission.ManageChannels)]
        [Command("cancel schedule"), Alias("cancel recurring")]
        [Summary("Removes a recurring session for this campaign")]
        public async Task<RuntimeResult> CancelScheduleAsync(
            [Summary("The date on which the session will take place.")] string date,
            [Summary("The time at which the session will take place.")] string time,
            [Summary("The campaign that the session belongs to.")] string campaign = null)
        {
            #region Validation

            #region Date

            if (!DateTime.TryParse($"{date} {time}", out var parsedDate))
                return GameMasterResult.ErrorResult("Invalid date.");

            #endregion

            #region Campaign

            string campaignId;
            if (campaign == null)
            {
                campaignId = Context.Channel.Name;
                var campaignTextChannel = Context.Guild.TextChannels.FirstOrDefault(chan => chan.Name == campaignId);
                if (campaignTextChannel == null)
                    return GameMasterResult.ErrorResult("Campaign does not exist on this server.");
            }
            else
            {
                campaignId = campaign.Replace(' ', '-').ToLower();
                var campaignTextChannel = Context.Guild.TextChannels.FirstOrDefault(chan => chan.Name == campaignId);
                if (campaignTextChannel == null)
                    return GameMasterResult.ErrorResult("Campaign does not exist on this server.");
            }

            #endregion

            #endregion

            try
            {
                await _service.CancelSchedule(campaignId, parsedDate);
                await ReplyAsync($"Session schedule on {parsedDate.ToShortDateString()} at {parsedDate.ToShortTimeString()} cancelled successfully.");
                return GameMasterResult.SuccessResult();
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult(e.Message);
            }
        }
    }
}
