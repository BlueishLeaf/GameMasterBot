using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;
using GameMasterBot.Data;
using GameMasterBot.Extensions;
using GameMasterBot.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace GameMasterBot.Services
{
    public class CampaignService
    {
        private readonly GameMasterContext _context;
        
        public CampaignService(GameMasterContext context) => _context = context;
        
        public async Task<Campaign> GetByTextChannelId(ulong textChannelId) => 
            await _context.Campaigns.AsQueryable().SingleAsync(c => c.TextChannelId == textChannelId);
        
        public async Task<IEnumerable<Campaign>> GetForServer(ulong guildId) => 
            await _context.Campaigns.AsQueryable().Where(c => c.GuildId == guildId).ToListAsync();
        
        public async Task<Campaign> Create(
            string name,
            string system,
            SocketUser user,
            SocketGuild guild,
            ulong textChannelId,
            ulong voiceChannelId,
            ulong playerRoleId,
            ulong gameMasterRoleId)
        {
            var userDb = await _context.Users.AddIfNotExists(new User
            {
                Id = user.Id,
                Username = user.Username
            }, u => u.Id == user.Id);
            var guildDb = await _context.Guilds.AddIfNotExists(new Guild
            {
                Id = guild.Id,
                Name = guild.Name
            }, g => g.Id == guild.Id);
            var campaignDb = (await _context.Campaigns.AddAsync(new Campaign
            {
                Name = name,
                System = system,
                User = userDb,
                Guild = guildDb,
                TextChannelId = textChannelId,
                VoiceChannelId = voiceChannelId,
                PlayerRoleId = playerRoleId,
                GameMasterRoleId = gameMasterRoleId
            })).Entity;
            await _context.SaveChangesAsync();
            return campaignDb;
        }

        public async Task<Campaign> AddPlayers(ulong id, IEnumerable<SocketGuildUser> guildUsers)
        {
            var campaign = await _context.Campaigns.AsQueryable().SingleAsync(c => c.Id == id);
            foreach (var guildUser in guildUsers)
            {
                var user = await _context.Users.AddIfNotExists(new User
                {
                    Id = guildUser.Id,
                    Username = guildUser.Username
                }, u => u.Id == guildUser.Id);
                campaign.CampaignUsers.Add(new CampaignUser
                {
                    CampaignId = campaign.Id,
                    UserId = user.Id,
                });
            }
            await _context.SaveChangesAsync();
            return campaign;
        }
        
        public async Task<Campaign> RemovePlayers(ulong id, IEnumerable<SocketGuildUser> guildUsers)
        {
            var campaign = await _context.Campaigns.AsQueryable().SingleAsync(c => c.Id == id);
            foreach (var guildUser in guildUsers)
            {
                var campaignUser = campaign.CampaignUsers.Single(cu => cu.CampaignId == campaign.Id && cu.UserId == guildUser.Id);
                campaign.CampaignUsers.Remove(campaignUser);
            }
            await _context.SaveChangesAsync();
            return campaign;
        }
        
        public async Task<Campaign> Update(Campaign campaign)
        {
            _context.Update(campaign);
            await _context.SaveChangesAsync();
            return campaign;
        }

        public async Task Remove(ulong id)
        {
            var campaign = await _context.Campaigns.AsQueryable().SingleAsync(c => c.Id == id);
            _context.Campaigns.Remove(campaign);
            await _context.SaveChangesAsync();
        }
        

    }
}
