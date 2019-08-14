using System.Collections.Generic;
using Entities.core;

namespace Common.Interfaces.Repositories
{
    public interface IPlayerRepository: IRepository<Player>
    {
        IEnumerable<Player> GetPlayersForServer(string serverId);
        IEnumerable<Player> GetPlayersForCampaign(string campaignId);
    }
}
