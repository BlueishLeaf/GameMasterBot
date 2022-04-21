using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;
using GameMasterBot.Data;
using GameMasterBot.Extensions;
using GameMasterBot.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using GameMasterBot.DTO;

namespace GameMasterBot.Services
{
    public class CampaignService
    {
        private readonly GameMasterBotContext _context;
        
        public CampaignService(GameMasterBotContext context) => _context = context;
        
        public async Task<Campaign?> GetByTextChannelId(ulong textChannelId) => 
            await _context.Campaigns.AsQueryable().SingleOrDefaultAsync(c => c.TextChannelId == textChannelId);
        
        public async Task<IEnumerable<Campaign>> GetAllByGuildId(ulong guildId) => 
            await _context.Campaigns.AsQueryable().Where(c => c.GuildId == guildId).ToListAsync();
        
        public async Task<Campaign> Create(CreateCampaignDto createCampaignDto)
        {
            var userDb = await _context.Users.AddIfNotExists(new User
            {
                DiscordId = createCampaignDto.User.Id,
                Username = createCampaignDto.User.Username
            }, u => u.DiscordId == createCampaignDto.User.Id);
            
            var guildDb = await _context.Guilds.AddIfNotExists(new Guild
            {
                DiscordId = createCampaignDto.Guild.Id,
                Name = createCampaignDto.Guild.Name
            }, g => g.DiscordId == createCampaignDto.Guild.Id);
            
            var campaignDb = (await _context.Campaigns.AddAsync(new Campaign
            {
                Name = createCampaignDto.Name,
                System = createCampaignDto.System,
                GameMaster = new GameMaster { User = userDb },
                Guild = guildDb,
                TextChannelId = createCampaignDto.TextChannelId,
                VoiceChannelId = createCampaignDto.VoiceChannelId,
                PlayerRoleId = createCampaignDto.PlayerRoleId,
                GameMasterRoleId = createCampaignDto.GameMasterRoleId
            })).Entity;
            
            await _context.SaveChangesAsync();
            return campaignDb;
        }

        public async Task<Campaign> AddPlayer(long id, SocketGuildUser guildUser)
        {
            var campaign = await _context.Campaigns.AsQueryable().SingleAsync(c => c.Id == id);
            var user = await _context.Users.AddIfNotExists(new User
            {
                DiscordId = guildUser.Id,
                Username = guildUser.Username
            }, u => u.DiscordId == guildUser.Id);
            campaign.Players.Add(new Player { User = user });
            await _context.SaveChangesAsync();
            return campaign;
        }
        
        public async Task<Campaign> RemovePlayer(long id, SocketGuildUser guildUser)
        {
            var campaign = await _context.Campaigns.AsQueryable().SingleAsync(c => c.Id == id);
            var campaignPlayer = campaign.Players.Single(p => p.User.DiscordId == guildUser.Id);
            campaign.Players.Remove(campaignPlayer);
            await _context.SaveChangesAsync();
            return campaign;
        }
        
        public async Task<Campaign> Update(Campaign campaign)
        {
            _context.Update(campaign);
            await _context.SaveChangesAsync();
            return campaign;
        }

        public async Task Remove(long id)
        {
            var campaign = await _context.Campaigns.AsQueryable().SingleAsync(c => c.Id == id);
            _context.Campaigns.Remove(campaign);
            await _context.SaveChangesAsync();
        }

        public async Task<User> GetUserByDiscordUser(SocketGuildUser guildUser) =>
            await _context.Users.AddIfNotExists(new User
            {
                DiscordId = guildUser.Id,
                Username = guildUser.Username
            }, u => u.DiscordId == guildUser.Id);
    }
}
