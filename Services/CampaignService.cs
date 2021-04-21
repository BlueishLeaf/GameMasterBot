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
        
        public async Task<Campaign?> GetByTextChannelId(ulong textChannelId) => 
            await _context.Campaigns.AsQueryable().SingleOrDefaultAsync(c => c.TextChannelId == textChannelId);
        
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
                DiscordId = user.Id,
                Username = user.Username
            }, u => u.DiscordId == user.Id);
            var guildDb = await _context.Guilds.AddIfNotExists(new Guild
            {
                DiscordId = guild.Id,
                Name = guild.Name
            }, g => g.DiscordId == guild.Id);
            var campaignDb = (await _context.Campaigns.AddAsync(new Campaign
            {
                Name = name,
                System = system,
                GameMaster = new GameMaster { User = userDb },
                Guild = guildDb,
                TextChannelId = textChannelId,
                VoiceChannelId = voiceChannelId,
                PlayerRoleId = playerRoleId,
                GameMasterRoleId = gameMasterRoleId
            })).Entity;
            await _context.SaveChangesAsync();
            return campaignDb;
        }

        public async Task<Campaign> AddPlayers(long id, IEnumerable<SocketGuildUser> guildUsers)
        {
            var campaign = await _context.Campaigns.AsQueryable().SingleAsync(c => c.Id == id);
            foreach (var guildUser in guildUsers)
            {
                var user = await _context.Users.AddIfNotExists(new User
                {
                    DiscordId = guildUser.Id,
                    Username = guildUser.Username
                }, u => u.DiscordId == guildUser.Id);
                campaign.Players.Add(new Player { User = user });
            }
            await _context.SaveChangesAsync();
            return campaign;
        }
        
        public async Task<Campaign> RemovePlayers(long id, IEnumerable<SocketGuildUser> guildUsers)
        {
            var campaign = await _context.Campaigns.AsQueryable().SingleAsync(c => c.Id == id);
            foreach (var guildUser in guildUsers)
            {
                var campaignPlayer = campaign.Players.Single(p => p.User.DiscordId == guildUser.Id);
                campaign.Players.Remove(campaignPlayer);
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
