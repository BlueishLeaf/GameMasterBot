using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameMasterBot.Data;
using GameMasterBot.Extensions;
using GameMasterBot.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using GameMasterBot.DTOs;
using GameMasterBot.Services.Interfaces;

namespace GameMasterBot.Services
{
    public class CampaignService(GameMasterBotContext context) : ICampaignService
    {
        public async Task<Campaign> GetByTextChannelId(ulong textChannelId)
        {
            // await using var dbContext = new GameMasterBotContext();
            return await context.Campaigns.Include(c => c.Sessions)
                .SingleOrDefaultAsync(c => c.TextChannelId == textChannelId);
        }

        public async Task<IEnumerable<Campaign>> GetAllByGuildId(ulong guildId) => 
            await context.Campaigns.Include(c => c.Guild)
                .Where(c => c.Guild.DiscordId == guildId)
                .ToListAsync();
        
        public async Task<Campaign> Create(CreateCampaignDto createCampaignDto)
        {
            var userDb = await context.Users.FetchOrAddIfNotExists(new User
            {
                DiscordId = createCampaignDto.UserDiscordId,
                TimeZoneId = "UTC"
            }, u => u.DiscordId == createCampaignDto.UserDiscordId);
            
            var guildDb = await context.Guilds.FetchOrAddIfNotExists(new Guild
            {
                DiscordId = createCampaignDto.GuildDiscordId
            }, g => g.DiscordId == createCampaignDto.GuildDiscordId);
            
            var campaignDb = (await context.Campaigns.AddAsync(new Campaign
            {
                Name = createCampaignDto.Name,
                System = createCampaignDto.System,
                CreatedAt = DateTime.UtcNow,
                GameMaster = new GameMaster { User = userDb },
                Guild = guildDb,
                TextChannelId = createCampaignDto.TextChannelId,
                VoiceChannelId = createCampaignDto.VoiceChannelId,
                PlayerRoleId = createCampaignDto.PlayerRoleId,
                GameMasterRoleId = createCampaignDto.GameMasterRoleId
            })).Entity;
            
            await context.SaveChangesAsync();
            return campaignDb;
        }

        public async Task<Campaign> AddPlayer(long id, ulong playerDiscordId)
        {
            var campaign = await context.Campaigns.SingleAsync(c => c.Id == id);
            var user = await context.Users.FetchOrAddIfNotExists(new User
            {
                DiscordId = playerDiscordId,
                TimeZoneId = "UTC"
            }, u => u.DiscordId == playerDiscordId);
            campaign.Players.Add(new CampaignPlayer { User = user });
            await context.SaveChangesAsync();
            return campaign;
        }
        
        public async Task<Campaign> RemovePlayer(long id, ulong playerDiscordId)
        {
            var campaign = await context.Campaigns.SingleAsync(c => c.Id == id);
            var campaignPlayer = campaign.Players.Single(p => p.User.DiscordId == playerDiscordId);
            campaign.Players.Remove(campaignPlayer);
            await context.SaveChangesAsync();
            return campaign;
        }
        
        public async Task<Campaign> Update(Campaign campaign)
        {
            context.Update(campaign);
            await context.SaveChangesAsync();
            return campaign;
        }

        public async Task DeleteById(long id)
        {
            var campaign = await context.Campaigns.SingleAsync(c => c.Id == id);
            context.Campaigns.Remove(campaign);
            await context.SaveChangesAsync();
        }
    }
}
