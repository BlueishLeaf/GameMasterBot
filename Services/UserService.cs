using System.Threading.Tasks;
using GameMasterBot.Data;
using GameMasterBot.Extensions;
using GameMasterBot.Models.Entities;
using GameMasterBot.Services.Interfaces;

namespace GameMasterBot.Services
{
    public class UserService(GameMasterBotContext context) : IUserService
    {
        public async Task<User> GetByDiscordUserId(ulong discordUserId)
        {
            var user = await context.Users.FetchOrAddIfNotExists(new User
            {
                DiscordId = discordUserId,
                TimeZoneId = "UTC"
            }, u => u.DiscordId == discordUserId);
            
            await context.SaveChangesAsync();
            return user;
        }

        public async Task UpdateTimezone(ulong discordUserId, string timezone)
        {
            var userDb = await context.Users.FetchOrAddIfNotExists(new User
            {
                DiscordId = discordUserId,
                TimeZoneId = "UTC"
            }, u => u.DiscordId == discordUserId);
            
            userDb.TimeZoneId = timezone;
            await context.SaveChangesAsync();
        }
    }
}