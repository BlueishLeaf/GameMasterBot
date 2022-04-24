using System;
using System.Threading.Tasks;
using Discord.Interactions;
using GameMasterBot.Extensions;
using GameMasterBot.Services;
using GameMasterBot.Services.Interfaces;
// Modules and their methods are picked up by the handler but not recognised by Rider
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace GameMasterBot.Modules
{
    [RequireContext(ContextType.Guild)]
    [Group("timezone", "Commands for managing users' timezones.")]
    public class TimezoneModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly IUserService _userService;
        
        public TimezoneModule(IUserService userService) => _userService = userService;

        [SlashCommand("view", "Displays the timezone that you have set for yourself.")]
        public async Task<RuntimeResult> TimezoneAsync()
        {
            var user = await _userService.GetByDiscordUserId(Context.User.Id);
            if (user.TimeZoneId == null)
            {
                return CommandResult.FromError("You have not set a timezone yet. You can set one using '/set-timezone'.");
            }

            // Search for timezone object
            TimeZoneInfo tzInfo;
            try
            {
                tzInfo = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZoneId);
            }
            catch (TimeZoneNotFoundException exception)
            {
                Console.WriteLine(exception);
                return CommandResult.FromError("Sorry, I could not find your timezone. Please look for it under the 'TZ database name' column on this list https://en.wikipedia.org/wiki/List_of_tz_database_time_zones and try again.");
            }

            await RespondAsync($"Your timezone is '{tzInfo.StandardName}'.", ephemeral: true);
            return CommandResult.AsSuccess();
        }

        [SlashCommand("set", "Sets your timezone to make scheduling sessions easier.")]
        public async Task<RuntimeResult> SetTimezoneAsync(
            [Summary("timezone", "Your IANA timezone (case-sensitive). Use '/timezone list' to find your IANA timezone.")] string tz)
        {
            // Search for timezone object
            TimeZoneInfo tzInfo;
            try
            {
                tzInfo = TimeZoneInfo.FindSystemTimeZoneById(tz);
            }
            catch (TimeZoneNotFoundException exception)
            {
                Console.WriteLine(exception);
                return CommandResult.FromError("Sorry, I could not find your timezone. Please look for it under the 'TZ database name' column on this list https://en.wikipedia.org/wiki/List_of_tz_database_time_zones and try again.");
            }

            await _userService.UpdateTimezone(Context.User.Id, tzInfo.Id);

            await RespondAsync($"Successfully set your timezone to {tzInfo.Id}.", ephemeral: true);
            return CommandResult.AsSuccess();
        }
        
        [SlashCommand("view-all", "Displays all timezones compatible with this bot.")]
        public async Task<RuntimeResult> ShowTimezonesAsync()
        {
            await RespondAsync("View timezones compatible with '/set-timezone' here: https://en.wikipedia.org/wiki/List_of_tz_database_time_zones", ephemeral: true);
            return CommandResult.AsSuccess();
        }
        
        [SlashCommand("convert", "Converts a UTC time to your local time.")]
        public async Task<RuntimeResult> ConvertAsync(
            [Summary("utc-time", "The time in UTC that you want to convert.")] string utcTime)
        {
            var user = await _userService.GetByDiscordUserId(Context.User.Id);

            if (user.TimeZoneId == null)
            {
                return CommandResult.FromError("You have not set a timezone yet. Set one with '/set-timezone' followed by your timezone abbreviation.");
            }

            if (!DateTime.TryParse(utcTime, out var parsedTime))
            {
                return CommandResult.FromError("You entered an invalid time. Time must be in the form 'HH:mm'.");
            }

            TimeZoneInfo tzInfo;
            try
            {
                tzInfo = TimeZoneInfo.FindSystemTimeZoneById(user.TimeZoneId);
            }
            catch (TimeZoneNotFoundException exception)
            {
                Console.WriteLine(exception);
                return CommandResult.FromError("Sorry, I could not find your timezone. Please look for it under the 'TZ database name' column on this list https://en.wikipedia.org/wiki/List_of_tz_database_time_zones and try again.");
            }

            var localTime = TimeZoneInfo.ConvertTimeFromUtc(parsedTime, tzInfo);

            await RespondAsync($"{parsedTime:HH:mm} UTC = {localTime:HH:mm} {tzInfo.StandardName}.", ephemeral: true);
            return CommandResult.AsSuccess();
        } 
    }
}