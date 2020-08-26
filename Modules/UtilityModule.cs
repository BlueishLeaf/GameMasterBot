using System;
using System.Threading.Tasks;
using Discord.Commands;
using GameMasterBot.Extensions;
using GameMasterBot.Services;
using TimeZoneConverter;

namespace GameMasterBot.Modules
{
    [RequireContext(ContextType.Guild)]
    [Name("Utility")]
    public class UtilityModule : ModuleBase<SocketCommandContext>
    {
        private readonly UserService _userService;
        
        public UtilityModule(UserService userService)
        {
            _userService = userService;
        }
        
        [Command("timezone")]
        [Summary("View the timezone that you have set.")]
        public async Task<RuntimeResult> TimezoneAsync()
        {
            var user = await _userService.GetByDiscordUser(Context.User);
            if (user.TimeZoneId == null) return GameMasterResult.ErrorResult("you have not set a timezone yet. You can set one using '!set-timezone'."); 
            // Search for timezone object
            if (!TZConvert.TryGetTimeZoneInfo(user.TimeZoneId, out var tzInfo)) return GameMasterResult.ErrorResult("I could not find that timezone.");
            
            await ReplyAsync($"Your timezone is '{tzInfo.StandardName}'.");
            return GameMasterResult.SuccessResult();
        }
        
        [Command("set-timezone")]
        [Summary("Adds a timezone role to assist with scheduling.")]
        public async Task<RuntimeResult> SetTimezoneAsync(
            [Summary("The timezone.")] string tz)
        {
            // Search for timezone object
            if (!TZConvert.TryGetTimeZoneInfo(tz, out var tzInfo)) return GameMasterResult.ErrorResult("I could not find that timezone.");

            await _userService.UpdateTimezone(Context.User, tzInfo.Id);

            await ReplyAsync($"Successfully set your timezone.");
            return GameMasterResult.SuccessResult();
        }
        
        [Command("timezones")]
        [Summary("Displays all timezones compatible with this bot.")]
        public async Task<RuntimeResult> ShowTimezonesAsync()
        {
            await ReplyAsync("View timezones compatible with `!set-timezone` here: https://github.com/BlueishLeaf/GameMasterBot/blob/master/TIMEZONES.md");
            return GameMasterResult.SuccessResult();
        }
        
        [Command("convert")]
        [Summary("Converts a UTC time to your local time.")]
        public async Task<RuntimeResult> ConvertAsync(
            [Summary("The UTC time.")] string utcTime)
        {
            var user = await _userService.GetByDiscordUser(Context.User);

            if (!DateTime.TryParse(utcTime, out var parsedTime))
                return GameMasterResult.ErrorResult("you entered an invalid date.");
            
            if (!TZConvert.TryGetTimeZoneInfo(user.TimeZoneId, out var tzInfo))
                return GameMasterResult.ErrorResult("your timezone was not found.");

            var localTime = TimeZoneInfo.ConvertTimeFromUtc(parsedTime, tzInfo);

            await ReplyAsync($"{parsedTime:HH:mm} UTC = {localTime:HH:mm} {tzInfo.Id}.");
            return GameMasterResult.SuccessResult();
        } 
    }
}