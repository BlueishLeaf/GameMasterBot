using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord.Interactions;
using GameMasterBot.Embeds;
using GameMasterBot.Extensions;
using GameMasterBot.Models.Enums;
using GameMasterBot.Services;
using TimeZoneConverter;

namespace GameMasterBot.Modules
{
    [RequireContext(ContextType.Guild)]
    [Group("session", "Commands for managing the sessions of a campaign.")]
    public class SessionModule : InteractionModuleBase<SocketInteractionContext>
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
        
        [SlashCommand("add", "Schedule a new standalone session for this campaign.")]
        public async Task<RuntimeResult> AddStandaloneSessionAsync(
            [Summary("The date on which the session will take place. Must be in the form 'dd/MM/yyyy'.")] string date,
            [Summary("The time at which the session will take place. Must be in the form 'hh:mm'.")] string time)
        {
            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);
            if (campaign == null)
            {
                return CommandResult.FromError("You must be in your campaign's text channel to schedule a standalone session.");
            }

            var commandIssuer = Context.Guild.GetUser(Context.User.Id);
            if (campaign.GameMaster.User.DiscordId != Context.User.Id && !commandIssuer.GuildPermissions.Administrator)
            {
                return CommandResult.FromError("You do not have permission to add a session to this campaign. You must either be the game master of this campaign or a server administrator.");
            }

            if (!DateTime.TryParseExact($"{date} {time}", "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            {
                return CommandResult.FromError("You entered an invalid date or time. The date must be in the form 'dd/MM/yyyy' and the time must be in the form 'hh:mm'.");
            }

            var user = await _userService.GetByDiscordUser(Context.User);
            var timeZoneId = user.TimeZoneId;
            if (timeZoneId == null)
            {
                // await RespondAsync("You do not have your timezone set, so I will just assume you wrote that date in UTC. If you want me to account for your timezone, set it using the '/set-timezone' command.");
                timeZoneId = "UTC";
            }
            if (!TZConvert.TryGetTimeZoneInfo(timeZoneId, out var tzInfo))
            {
                return CommandResult.FromError("I could not find your timezone.");
            }

            var utcTime = TimeZoneInfo.ConvertTimeToUtc(parsedDate, tzInfo);
            if (utcTime <= DateTime.UtcNow)
            {
                return CommandResult.FromError("You cannot schedule a session in the past!");
            }

            var session = await _sessionService.Create(campaign.Id, ScheduleFrequency.Standalone, utcTime);
            await RespondAsync(embed: BotEmbeds.SessionInfo("Standalone session successfully scheduled for this campaign!", session));
            return CommandResult.AsSuccess();
        }
        
        [SlashCommand("schedule", "Schedules a new session for a campaign")]
        public async Task<RuntimeResult> ScheduleSessionAsync(
            [Summary("The date on which the session will take place. Must be in the form 'dd/MM/yyyy'.")] string date,
            [Summary("The time at which the session will take place. Must be in the form 'hh:mm'.")] string time,
            [Summary("The frequency of this schedule. Must be either 'weekly', 'fortnightly', or 'monthly'.")] string frequency)
        {
            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);
            if (campaign == null)
            {
                return CommandResult.FromError("You must be in your campaign's text channel to schedule a session.");
            }

            // Check to make sure that this user is the game master of the campaign
            var commandIssuer = Context.Guild.GetUser(Context.User.Id);
            if (campaign.GameMaster.User.DiscordId != Context.User.Id && !commandIssuer.GuildPermissions.Administrator)
            {
                return CommandResult.FromError("You do not have permission to schedule a session for this campaign. You must either be the game master of this campaign or a server administrator.");
            }

            if (!DateTime.TryParseExact($"{date} {time}", "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            {
                return CommandResult.FromError("You entered an invalid date or time. The date must be in the form 'dd/MM/yyyy' and the time must be in the form 'hh:mm'.");
            }
            
            var user = await _userService.GetByDiscordUser(Context.User);
            var timeZoneId = user.TimeZoneId;
            if (timeZoneId == null)
            {
                await RespondAsync("You do not have your timezone set, so I will just assume you wrote that date in UTC. If you want me to account for your timezone, set it using the '/set-timezone' command.");
                timeZoneId = "UTC";
            }           
            if (!TZConvert.TryGetTimeZoneInfo(timeZoneId, out var tzInfo))
            {
                return CommandResult.FromError("I could not find your timezone.");
            }

            var utcTime = TimeZoneInfo.ConvertTimeToUtc(parsedDate, tzInfo);

            if (utcTime <= DateTime.UtcNow)
            {
                return CommandResult.FromError("You cannot schedule a session in the past!");
            }

            var scheduleLower = frequency.ToLower();
            ScheduleFrequency scheduleFrequencyValue;
            switch (scheduleLower)
            {
                case "weekly":
                    scheduleFrequencyValue = ScheduleFrequency.Weekly;
                    break;
                case "fortnightly":
                    scheduleFrequencyValue = ScheduleFrequency.Fortnightly;
                    break;
                case "monthly":
                    scheduleFrequencyValue = ScheduleFrequency.Monthly;
                    break;
                default:
                    return CommandResult.FromError("You chose an invalid schedule frequency. Try again and choose from 'Weekly', 'Fortnightly', or 'Monthly'.");
            }
            
            var session = await _sessionService.Create(campaign.Id, scheduleFrequencyValue, utcTime);
            await RespondAsync(embed: BotEmbeds.SessionInfo("Session successfully scheduled for this campaign!", session));
            return CommandResult.AsSuccess();
        }
        
        [SlashCommand("next", "Display the details of the next scheduled session for this campaign.")]
        public async Task<RuntimeResult> NextSessionAsync()
        {
            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);
            if (campaign == null)
            {
                return CommandResult.FromError("You must be in your campaign's text channel to check the details of an upcoming session.");
            }

            var sessions = await _sessionService.GetUpcoming(campaign.Id);
            if (!sessions.Any())
            {
                return CommandResult.FromError("The next session for this campaign has not been scheduled yet.");
            }

            await RespondAsync(embed: BotEmbeds.SessionInfo($"Next session scheduled for {campaign.Name}", sessions.First()));
            return CommandResult.AsSuccess();
        }
        
        [SlashCommand("upcoming", "Display the details of all the upcoming sessions for this campaign.")]
        public async Task<RuntimeResult> UpcomingSessionsAsync()
        {
            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);
            if (campaign == null)
            {
                return CommandResult.FromError("You must be in your campaign's text channel to check the details of upcoming sessions.");
            }

            var sessions = await _sessionService.GetUpcoming(campaign.Id);
            if (!sessions.Any())
            {
                return CommandResult.FromError("The next session for this campaign has not been scheduled yet.");
            }

            await RespondAsync(embed: BotEmbeds.SessionList($"Upcoming Sessions for {campaign.Name}", sessions));
            return CommandResult.AsSuccess();
        }
        
        [SlashCommand("cancel-next", "Cancel the next scheduled session for this campaign")]
        public async Task<RuntimeResult> CancelNextSessionAsync()
        {
            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);
            if (campaign == null)
            {
                return CommandResult.FromError("You must be in your campaign's text channel to cancel the next scheduled session.");
            }

            var commandIssuer = Context.Guild.GetUser(Context.User.Id);
            if (campaign.GameMaster.User.DiscordId != Context.User.Id && !commandIssuer.GuildPermissions.Administrator)
            {
                return CommandResult.FromError("You do not have permission to cancel a session for this campaign. You must either be the game master of this campaign or a server administrator.");
            }

            await _sessionService.CancelNext(campaign.Id);
            // TODO: Modify this message to show what session was cancelled and when the next one will be
            await RespondAsync("Next session cancelled successfully.");
            return CommandResult.AsSuccess();
        }

        [SlashCommand("cancel-schedule", "Removes a recurring session for this campaign")]
        public async Task<RuntimeResult> CancelSessionScheduleAsync(
            [Summary("The date on which the session is scheduled to take place. Must be in the form 'dd/MM/yyyy'.")] string date)
        {
            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);
            if (campaign == null)
            {
                return CommandResult.FromError("You must be in your campaign's text channel to cancel a scheduled session.");
            }

            if (!DateTime.TryParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            {
                return CommandResult.FromError("You entered an invalid date or time. The date must be in the form 'dd/MM/yyyy'.");
            }


            // Check to make sure that this user is the game master of the campaign
            var commandIssuer = Context.Guild.GetUser(Context.User.Id);
            if (campaign.GameMaster.User.DiscordId != Context.User.Id && !commandIssuer.GuildPermissions.Administrator)
            {
                return CommandResult.FromError("You do not have permission to cancel a session for this campaign. You must either be the game master of this campaign or a server administrator.");
            }

            await _sessionService.CancelScheduleForDate(campaign.Id, parsedDate);
            await RespondAsync($"Session scheduled for {parsedDate.ToShortDateString()} cancelled successfully.");
            return CommandResult.AsSuccess();
        }
        
        [SlashCommand("cancel date", "Removes a planned session for this campaign")]
        public async Task<RuntimeResult> CancelSessionOnDateAsync(
            [Summary("The date on which the session will take place.")] string date)
        {
            var campaign = await _campaignService.GetByTextChannelId(Context.Channel.Id);
            if (campaign == null)
            {
                return CommandResult.FromError("you are not in a campaign text channel.");
            }

            if (!DateTime.TryParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
            {
                return CommandResult.FromError("the date you entered was invalid.");
            }

            // Check to make sure that this user is the game master of the campaign
            var commandIssuer = Context.Guild.GetUser(Context.User.Id);
            if (campaign.GameMaster.User.DiscordId != Context.User.Id && !commandIssuer.GuildPermissions.Administrator)
            {
                return CommandResult.FromError("you do not have permission to cancel a session for this campaign. You must either be the Game Master of this campaign or a Server Administrator.");
            }

            await _sessionService.CancelForDate(campaign.Id, parsedDate);
            await RespondAsync($"Session planned for {parsedDate.ToShortDateString()} cancelled successfully.");
            return CommandResult.AsSuccess();
        }
    }
}
