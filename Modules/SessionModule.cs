using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord.Interactions;
using GameMasterBot.Embeds;
using GameMasterBot.Extensions;
using GameMasterBot.Models.Enums;
using GameMasterBot.Services;

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

        [SlashCommand("schedule", "Schedules a new session for a campaign.")]
        public async Task<RuntimeResult> ScheduleSessionAsync(
            [Summary("date","The date on which the session will take place. Must be in the form 'dd/MM/yyyy'.")] string date,
            [Summary("time","The time at which the session will take place. Must be in the form 'hh:mm'.")] string time,
            [Summary("frequency","The frequency of this schedule. Must be either 'weekly', 'fortnightly', or 'monthly'.")]
            [Choice("Standalone", 0)]
            [Choice("Weekly", 1)]
            [Choice("Fortnightly", 2)]
            [Choice("Monthly", 3)]
            ScheduleFrequency frequency)
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
            
            var user = await _userService.GetByDiscordUserId(Context.User.Id);
            var timeZoneId = user.TimeZoneId;
            if (timeZoneId == null)
            {
                await RespondAsync("You do not have your timezone set, so I will just assume you wrote that date in UTC. If you want me to account for your timezone, set it using the '/set-timezone' command.");
                timeZoneId = "UTC";
            }    
            
            TimeZoneInfo tzInfo;
            try
            {
                tzInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            }
            catch (TimeZoneNotFoundException exception)
            {
                Console.WriteLine(exception);
                return CommandResult.FromError("Sorry, I could not find your timezone. Please look for it under the 'TZ database name' column on this list https://en.wikipedia.org/wiki/List_of_tz_database_time_zones and try again.");
            }

            var utcTime = TimeZoneInfo.ConvertTimeToUtc(parsedDate, tzInfo);

            if (utcTime <= DateTime.UtcNow)
            {
                return CommandResult.FromError("You cannot schedule a session in the past!");
            }
            
            var session = await _sessionService.Create(campaign.Id, frequency, utcTime);
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

        [SlashCommand("cancel", "Cancel the next scheduled session for this campaign.")]
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

            var sessions = await _sessionService.GetUpcoming(campaign.Id);
            if (!sessions.Any())
            {
                return CommandResult.FromError("The next session for this campaign has not been scheduled yet.");
            }
            
            await _sessionService.CancelNext(campaign.Id);
            // TODO: Modify this message to show what session was cancelled and when the next one will be
            await RespondAsync("Next session cancelled successfully.");
            return CommandResult.AsSuccess();
        }
    }
}
