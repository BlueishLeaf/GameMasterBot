using System.Collections.Generic;
using System.Threading.Tasks;
using GameMasterBot.DTO;
using GameMasterBot.Models.Entities;

namespace GameMasterBot.Services;

public interface ICampaignService
{
    Task<Campaign> GetByTextChannelId(ulong textChannelId);
    Task<IEnumerable<Campaign>> GetAllByGuildId(ulong guildId);
    Task<Campaign> Create(CreateCampaignDto createCampaignDto);
    Task<Campaign> AddPlayer(long id, ulong playerDiscordId);
    Task<Campaign> RemovePlayer(long id, ulong playerDiscordId);
    Task<Campaign> Update(Campaign campaign);
    Task DeleteById(long id);
}