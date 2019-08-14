using System.Collections.Generic;
using Entities.core;

namespace Common.Interfaces.Repositories
{
    public interface ICampaignRepository: IRepository<Campaign>
    {
        IEnumerable<Campaign> GetCampaignsForServer(string serverId);
        IEnumerable<Campaign> GetCampaignsForPlayer(string playerId);
    }
}
