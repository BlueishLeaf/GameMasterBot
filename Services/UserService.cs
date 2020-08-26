using System.Threading.Tasks;
using Discord.WebSocket;
using GameMasterBot.Data;
using GameMasterBot.Extensions;
using GameMasterBot.Models.Entities;

namespace GameMasterBot.Services
{
    public class UserService
    {
        private readonly GameMasterContext _context;

        public UserService(GameMasterContext context) => _context = context;

        public async Task<User> GetByDiscordUser(SocketUser socketUser)
        {
            var user = await _context.Users.AddIfNotExists(new User
            {
                Id = socketUser.Id,
                Username = socketUser.Username
            }, u => u.Id == socketUser.Id);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task UpdateTimezone(SocketUser user, string timezone)
        {
            var userDb = await _context.Users.AddIfNotExists(new User
            {
                Id = user.Id,
                Username = user.Username
            }, u => u.Id == user.Id);
            userDb.TimeZoneId = timezone;
            await _context.SaveChangesAsync();
        }
    }
}