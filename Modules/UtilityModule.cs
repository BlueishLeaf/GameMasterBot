using System;
using System.Threading.Tasks;
using Discord.Interactions;
using GameMasterBot.Extensions;
using GameMasterBot.Services;
using TimeZoneConverter;

namespace GameMasterBot.Modules
{
    [RequireContext(ContextType.Guild)]
    public class UtilityModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly UserService _userService;
        
        public UtilityModule(UserService userService) => _userService = userService;

        [SlashCommand("timezone", "View the timezone that you have set for yourself.")]
        public async Task<RuntimeResult> TimezoneAsync()
        {
            var user = await _userService.GetByDiscordUser(Context.User);
            if (user.TimeZoneId == null)
            {
                return CommandResult.FromError("You have not set a timezone yet. You can set one using '/set-timezone'.");
            }

            // Search for timezone object
            if (!TZConvert.TryGetTimeZoneInfo(user.TimeZoneId, out var tzInfo))
            {
                return CommandResult.FromError("I could not find that timezone, please try again.");
            }

            await RespondAsync($"Your timezone is '{tzInfo.StandardName}'.", ephemeral: true);
            return CommandResult.AsSuccess();
        }

        [SlashCommand("set-timezone", "Set your timezone to make scheduling sessions easier.")]
        public async Task<RuntimeResult> SetTimezoneAsync(
            [Summary("The timezone.")] string tz)
        {
            // Search for timezone object
            if (!TZConvert.TryGetTimeZoneInfo(tz, out var tzInfo))
            {
                return CommandResult.FromError("I could not find that timezone.");
            }

            await _userService.UpdateTimezone(Context.User, tzInfo.Id);

            await RespondAsync($"Successfully set your timezone to {tzInfo.StandardName}.", ephemeral: true);
            return CommandResult.AsSuccess();
        }
        
        [SlashCommand("timezones", "Display all timezones compatible with this bot.")]
        public async Task<RuntimeResult> ShowTimezonesAsync()
        {
            await RespondAsync("View timezones compatible with '/set-timezone' here: https://en.wikipedia.org/wiki/List_of_tz_database_time_zones");
            return CommandResult.AsSuccess();
        }
        
        [SlashCommand("convert", "Convert a UTC time to your local time.")]
        public async Task<RuntimeResult> ConvertAsync(
            [Summary("The UTC time.")] string utcTime)
        {
            var user = await _userService.GetByDiscordUser(Context.User);

            if (user.TimeZoneId == null)
            {
                return CommandResult.FromError("You have not set a timezone yet. Set one with '/set-timezone' followed by your timezone abbreviation.");
            }

            if (!DateTime.TryParse(utcTime, out var parsedTime))
            {
                return CommandResult.FromError("You entered an invalid time. Time must be in the form 'HH:mm'.");
            }

            if (!TZConvert.TryGetTimeZoneInfo(user.TimeZoneId, out var tzInfo))
            {
                return CommandResult.FromError("I could not find your timezone.");
            }

            var localTime = TimeZoneInfo.ConvertTimeFromUtc(parsedTime, tzInfo);

            await RespondAsync($"{parsedTime:HH:mm} UTC = {localTime:HH:mm} {tzInfo.StandardName}.");
            return CommandResult.AsSuccess();
        } 
    }
}