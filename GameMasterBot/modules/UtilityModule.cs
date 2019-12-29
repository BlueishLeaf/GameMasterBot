using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using GameMasterBot.Utils;
using TimeZoneConverter;

namespace GameMasterBot.modules
{
    [RequireContext(ContextType.Guild)]
    [Name("Utility")]
    public class UtilityModule : ModuleBase<SocketCommandContext>
    {
        [Command("timezone"), Alias("tz")]
        [Summary("Adds a timezone role to assist with scheduling.")]
        public async Task<RuntimeResult> AddTimezoneAsync(
            [Summary("The timezone.")] string tz)
        {
            // Search for timezone object
            if (!TZConvert.TryGetTimeZoneInfo(tz, out var tzInfo))
                return GameMasterResult.ErrorResult("Could not find timezone.");
            
            // Create timezone role if one does not exist
            var tzRole = Context.Guild.Roles.FirstOrDefault(role => role.Name == $"Timezone: {tzInfo.Id}") ??
                         (IRole) Context.Guild.CreateRoleAsync($"Timezone: {tzInfo.Id}", null, Color.Default).Result;

            // Get the discord user who issued this command
            var guildUser = Context.Guild.Users.FirstOrDefault(user => user.Id == Context.User.Id);
            if (guildUser == null)
                return GameMasterResult.ErrorResult("Could not find you in the server.");

            // If they have a timezone role already, delete it
            var oldRole = guildUser.Roles.FirstOrDefault(role => role.Name.Contains("Timezone:"));
            if (oldRole != null)
                await guildUser.RemoveRoleAsync(oldRole);

            // Add the timezone role to the user
            await guildUser.AddRoleAsync(tzRole);

            await ReplyAsync($"Successfully set the timezone of {guildUser.Username} to {tzInfo.StandardName}.");
            return GameMasterResult.SuccessResult();
        }
        
        [Command("timezones"), Alias("tzs")]
        [Summary("Displays all timezones compatible with this bot.")]
        public async Task<RuntimeResult> ShowTimezonesAsync()
        {
            await ReplyAsync("View timezones compatible with `!timezone` here: https://github.com/BlueishLeaf/GameMasterBot/blob/master/TIMEZONES.md");
            return GameMasterResult.SuccessResult();
        }
        
        [Command("convert"), Alias("cvt")]
        [Summary("Converts a UTC time to your local time.")]
        public async Task<RuntimeResult> ConvertAsync(
            [Summary("The UTC time.")] string utcTime)
        {
            var guildUser = Context.Guild.Users.FirstOrDefault(user => user.Id == Context.User.Id);
            if (guildUser == null)
                return GameMasterResult.ErrorResult("Could not find you in the server.");

            var tzRole = guildUser.Roles.FirstOrDefault(role => role.Name.Contains("Timezone:"));
            if (tzRole == null)
                return GameMasterResult.ErrorResult("Please add a timezone role using `!timezone 'your timezone'`");

            if (!DateTime.TryParse(utcTime, out var parsedTime))
                return GameMasterResult.ErrorResult("Invalid date.");
            
            var tzId = tzRole.Name.Remove(0, 10);
            if (!TZConvert.TryGetTimeZoneInfo(tzId, out var tzInfo))
                return GameMasterResult.ErrorResult("Timezone not found.");

            var localTime = TimeZoneInfo.ConvertTimeFromUtc(parsedTime, tzInfo);

            await ReplyAsync($"{parsedTime:HH:mm} UTC = {localTime:HH:mm} {tzRole.Name.Remove(0, 10)}.");
            return GameMasterResult.SuccessResult();
        } 
    }
}