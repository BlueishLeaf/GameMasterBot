using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using GameMasterBot.Embeds;
using GameMasterBot.Extensions;
using GameMasterBot.Models.Enums;
using GameMasterBot.Services;
using TimeZoneConverter;

namespace GameMasterBot.Modules
{
    [RequireContext(ContextType.Guild)]
    [Group("session"), Name("Session")]
    public class SessionModule: ModuleBase<SocketCommandContext>
    {
        private readonly SessionService _sessionService;
        private readonly CampaignService _campaignService;
        private readonly UserService _userService;

        public SessionModule(SessionService sessionService, CampaignService campaignService, UserService userService)
        {
            _sessionService = sessionService;
            _campaignService = campaignService;
            _userService = userService;
        }
        
        [Command("add")]
        [Summary("Creates a new adhoc session for this campaign.")]
        public async Task<RuntimeResult> AddAsync(
            [Summary("The date on which the session will take place.")] string date,
            [Summary("The time at which the session will take place.")] string time)
        {
            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);

            var commandIssuer = Context.Guild.GetUser(Context.User.Id);
            if (campaign.UserId != Context.User.Id && !commandIssuer.GuildPermissions.Administrator)
                return GameMasterResult.ErrorResult("you do not have permission to add a session to this campaign. You must either be the Game Master of this campaign or a Server Administrator.");

            if (!DateTime.TryParseExact($"{date} {time}", "dd/MM/yyyy hh:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                return GameMasterResult.ErrorResult("you entered an invalid date.");

            var user = await _userService.GetByDiscordUser(Context.User);
            var timeZoneId = user.TimeZoneId;
            if (timeZoneId == null)
            {
                await ReplyAsync("You do not have your timezone set, so I will just assume you wrote that date in UTC. If you want me to account for your timezone, set it using the '!timezone' command.");
                timeZoneId = "UTC";
            }
            if (!TZConvert.TryGetTimeZoneInfo(timeZoneId, out var tzInfo))
                return GameMasterResult.ErrorResult("your timezone was not found.");
        
            var utcTime = TimeZoneInfo.ConvertTimeToUtc(parsedDate, tzInfo);

            try
            {
                var session = await _sessionService.Create(campaign.Id, Schedule.AdHoc, utcTime);
                await ReplyAsync(embed: EmbedBuilder.SessionInfo($"Session successfully added for this campaign.", session));
                return GameMasterResult.SuccessResult();
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult(e.Message);
            }
        }
        
        [Command("schedule")]
        [Summary("Schedules a new session for a campaign")]
        public async Task<RuntimeResult> ScheduleAsync(
            [Summary("The date on which the session will take place.")] string date,
            [Summary("The time at which the session will take place.")] string time,
            [Summary("The schedule type for the session.")] string schedule)
        {
            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);

            // Check to make sure that this user is the game master of the campaign
            var commandIssuer = Context.Guild.GetUser(Context.User.Id);
            if (campaign.UserId != Context.User.Id && !commandIssuer.GuildPermissions.Administrator)
                return GameMasterResult.ErrorResult("you do not have permission to schedule a session for this campaign. You must either be the Game Master of this campaign or a Server Administrator.");

            if (!DateTime.TryParseExact($"{date} {time}", "dd/MM/yyyy hh:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                return GameMasterResult.ErrorResult("you entered an invalid date.");

            var user = await _userService.GetByDiscordUser(Context.User);
            var timeZoneId = user.TimeZoneId;
            if (timeZoneId == null)
            {
                await ReplyAsync("You do not have your timezone set, so I will just assume you wrote that date in UTC. If you want me to account for your timezone, set it using the '!set-timezone' command.");
                timeZoneId = "UTC";
            }           
            if (!TZConvert.TryGetTimeZoneInfo(timeZoneId, out var tzInfo))
                return GameMasterResult.ErrorResult("your timezone was not found.");
            var utcTime = TimeZoneInfo.ConvertTimeToUtc(parsedDate, tzInfo);

            var scheduleLower = schedule.ToLower();
            Schedule scheduleValue;
            switch (scheduleLower)
            {
                case "weekly":
                    scheduleValue = Schedule.Weekly;
                    break;
                case "fortnightly":
                    scheduleValue = Schedule.Fortnightly;
                    break;
                case "monthly":
                    scheduleValue = Schedule.Monthly;
                    break;
                default:
                    return GameMasterResult.ErrorResult("you chose an invalid schedule type. Choose from 'Weekly', 'Fortnightly', or 'Monthly'.");
            }

            try
            {
                var session = await _sessionService.Create(campaign.Id, scheduleValue, utcTime);
                await ReplyAsync(embed: EmbedBuilder.SessionInfo($"Session scheduled for this campaign.", session));
                return GameMasterResult.SuccessResult();
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult(e.Message);
            }
        }
        
        [Command("next")]
        [Summary("Get the next session for this campaign.")]
        public async Task<RuntimeResult> NextAsync()
        {
            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);
            try
            {
                var sessions = await _sessionService.GetUpcoming(campaign.Id);
                if (!sessions.Any())
                    return GameMasterResult.ErrorResult("the next session for this campaign has not been scheduled yet.");
                await ReplyAsync(embed: EmbedBuilder.SessionInfo($"Next session for this campaign.", sessions.First()));
                return GameMasterResult.SuccessResult();
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult(e.Message);
            }
        }
        
        [Command("upcoming")]
        [Summary("Get all the upcoming sessions for this campaign.")]
        public async Task<RuntimeResult> UpcomingAsync()
        {
            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);

            try
            {
                var sessions = await _sessionService.GetUpcoming(campaign.Id);
                if (!sessions.Any())
                    return GameMasterResult.ErrorResult("the next session for this campaign has not been scheduled yet.");
        
                await ReplyAsync(embed: EmbedBuilder.SessionList($"Upcoming Sessions for {campaign}", sessions));
                return GameMasterResult.SuccessResult();
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult(e.Message);
            }
        }
        
        [Command("cancel next")]
        [Summary("Cancels the next session for a campaign")]
        public async Task<RuntimeResult> CancelNextAsync()
        {
            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);

            var commandIssuer = Context.Guild.GetUser(Context.User.Id);
            if (campaign.Id != Context.User.Id && !commandIssuer.GuildPermissions.Administrator)
                return GameMasterResult.ErrorResult("you do not have permission to cancel a session for this campaign. You must either be the Game Master of this campaign or a Server Administrator.");

            try
            {
                await _sessionService.CancelNext(campaign.Id);
                await ReplyAsync("Next session cancelled successfully.");
                return GameMasterResult.SuccessResult();
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult(e.Message);
            }
        }

        [Command("cancel schedule")]
        [Summary("Removes a recurring session for this campaign")]
        public async Task<RuntimeResult> CancelScheduleAsync(
            [Summary("The date on which the session will take place.")] string date)
        {
            if (!DateTime.TryParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                return GameMasterResult.ErrorResult("the date you entered was invalid.");

            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);

            // Check to make sure that this user is the game master of the campaign
            var commandIssuer = Context.Guild.GetUser(Context.User.Id);
            if (campaign.Id != Context.User.Id && !commandIssuer.GuildPermissions.Administrator)
                return GameMasterResult.ErrorResult("you do not have permission to cancel a session for this campaign. You must either be the Game Master of this campaign or a Server Administrator.");

            try
            {
                await _sessionService.CancelScheduleForDate(campaign.Id, parsedDate);
                await ReplyAsync($"Session scheduled for {parsedDate.ToShortDateString()} cancelled successfully.");
                return GameMasterResult.SuccessResult();
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult(e.Message);
            }
        }
        
        [Command("cancel date")]
        [Summary("Removes a planned session for this campaign")]
        public async Task<RuntimeResult> CancelDateAsync(
            [Summary("The date on which the session will take place.")] string date)
        {
            if (!DateTime.TryParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
                return GameMasterResult.ErrorResult("the date you entered was invalid.");

            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);

            // Check to make sure that this user is the game master of the campaign
            var commandIssuer = Context.Guild.GetUser(Context.User.Id);
            if (campaign.Id != Context.User.Id && !commandIssuer.GuildPermissions.Administrator)
                return GameMasterResult.ErrorResult("you do not have permission to cancel a session for this campaign. You must either be the Game Master of this campaign or a Server Administrator.");

            try
            {
                await _sessionService.CancelForDate(campaign.Id, parsedDate);
                await ReplyAsync($"Session planned for {parsedDate.ToShortDateString()} cancelled successfully.");
                return GameMasterResult.SuccessResult();
            }
            catch (Exception e)
            {
                return GameMasterResult.ErrorResult(e.Message);
            }
        }
    }
}
