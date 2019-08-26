﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Interfaces.DataAccess;
using Common.Interfaces.Entities.Core;
using Entities.Core;

namespace GameMasterBot.Services
{
    public class CampaignService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CampaignService(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

        public ICampaign Create(string name, string system, string gameMaster, string url, string[] players, string createdBy, string guildName, ulong guildId)
        {
            // Build the campaign object from the params
            var campaign = new Campaign
            {
                Id = name.ToLower().Replace(' ', '-'),
                Name = name,
                System = system,
                GameMaster = gameMaster,
                Url = url,
                CreatedBy = createdBy,
                Players = new List<string>(players),
                ServerName = guildName,
                ServerId = guildId.ToString()
            };
            _unitOfWork.Campaigns.Add(campaign);
            return campaign;
        }

        public void Remove(string serverId, string campaignId) => _unitOfWork.Campaigns.Remove(serverId, campaignId);

        public async Task<ICampaign> Get(string serverId, string campaignId) => await _unitOfWork.Campaigns.Get(serverId, campaignId);

        public IEnumerable<ICampaign> GetForServer(string serverId) => _unitOfWork.Campaigns.GetForServer(serverId);

        public IEnumerable<ICampaign> GetForPlayer(string serverId, string playerName) => _unitOfWork.Campaigns.GetForPlayer(serverId, playerName);
    }
}