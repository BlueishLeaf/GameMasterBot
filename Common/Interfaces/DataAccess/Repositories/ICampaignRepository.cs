using System.Collections.Generic;
using Common.Interfaces.Entities.Core;

namespace Common.Interfaces.DataAccess.Repositories
{
    public interface ICampaignRepository: IRepository<ICampaign>
    {
        IEnumerable<ICampaign> GetCampaignsForServer(string serverId);
        IEnumerable<IPlayerCampaign> GetCampaignsForPlayer(string playerId);
    }
}
