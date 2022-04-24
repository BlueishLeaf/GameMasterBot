using System.Threading.Tasks;
using GameMasterBot.Models.Entities;

namespace GameMasterBot.Services.Interfaces;

public interface IUserService
{
    Task<User> GetByDiscordUserId(ulong discordUserId);
    Task UpdateTimezone(ulong discordUserId, string timezone);
}