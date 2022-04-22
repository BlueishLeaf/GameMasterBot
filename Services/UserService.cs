using System.Threading.Tasks;
using GameMasterBot.Data;
using GameMasterBot.Extensions;
using GameMasterBot.Models.Entities;

namespace GameMasterBot.Services
{
    public class UserService : IUserService
    {
        private readonly GameMasterBotContext _context;

        public UserService(GameMasterBotContext context) => _context = context;

        public async Task<User> GetByDiscordUserId(ulong discordUserId)
        {
            var user = await _context.Users.FetchOrAddIfNotExists(new User
            {
                DiscordId = discordUserId,
                TimeZoneId = "UTC"
            }, u => u.DiscordId == discordUserId);
            
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task UpdateTimezone(ulong discordUserId, string timezone)
        {
            var userDb = await _context.Users.FetchOrAddIfNotExists(new User
            {
                DiscordId = discordUserId,
                TimeZoneId = "UTC"
            }, u => u.DiscordId == discordUserId);
            
            userDb.TimeZoneId = timezone;
            await _context.SaveChangesAsync();
        }
    }
}