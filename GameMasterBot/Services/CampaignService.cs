using System.Collections.Generic;
using System.Linq;
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

        public ICampaign Create(string name, string system, string gameMaster, ulong gameMasterId, string url, IEnumerable<string> players, string createdBy, string guildName, ulong guildId)
        {
            // Build the campaign object from the params
            var campaign = new Campaign
            {
                Id = name.ToLower().Replace(' ', '-'),
                Name = name,
                System = system,
                GameMasterName = gameMaster,
                GameMasterId = gameMasterId,
                Url = url,
                CreatedBy = createdBy,
                Players = players.ToList(),
                ServerName = guildName,
                ServerId = guildId
            };
            _unitOfWork.Campaigns.Add(campaign);
            return campaign;
        }
        
        public ICampaign Create(ICampaign campaign)
        {
            _unitOfWork.Campaigns.Add(campaign);
            return campaign;
        }

        public void Remove(ulong serverId, string campaignId)
        {
            _unitOfWork.Campaigns.Remove(serverId, campaignId);
            // Delete any remnant session objects
            var sessions= _unitOfWork.Sessions.GetForCampaign(serverId, campaignId);
            _unitOfWork.Sessions.RemoveRange(sessions);
        }

        public async Task<ICampaign> Get(ulong serverId, string campaignId) => await _unitOfWork.Campaigns.Get(serverId, campaignId);

        public IEnumerable<ICampaign> GetForServer(ulong serverId) => _unitOfWork.Campaigns.GetForServer(serverId);

        public IEnumerable<ICampaign> GetForPlayer(ulong serverId, string playerName, string playerNickname) => _unitOfWork.Campaigns.GetForServer(serverId).Where(campaign => campaign.Players.Contains(playerName) || campaign.Players.Contains(playerNickname)  || campaign.GameMasterName == playerName || campaign.GameMasterName == playerNickname);
    }
}
