using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Interfaces.Entities.Core;

namespace Common.Interfaces.DataAccess.Repositories
{
    public interface ICampaignRepository
    {
        Task<ICampaign> Get(string serverId, string campaignId);
        IEnumerable<ICampaign> GetForServer(string serverId);
        IEnumerable<ICampaign> GetForPlayer(string serverId, string playerName);
        void Add(ICampaign campaign);
        void Remove(string serverId, string campaignId);
    }
}
