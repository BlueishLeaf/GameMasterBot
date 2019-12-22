using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Interfaces.Entities.Core;

namespace Common.Interfaces.DataAccess.Repositories
{
    public interface ICampaignRepository
    {
        Task<ICampaign> Get(ulong serverId, string campaignId);
        IEnumerable<ICampaign> GetForServer(ulong serverId);
        IEnumerable<ICampaign> GetForPlayer(ulong serverId, string playerName);
        void Add(ICampaign campaign);
        void Remove(ulong serverId, string campaignId);
    }
}
