using System.Collections.Generic;
using Common.Interfaces.Entities.Core;

namespace Common.Interfaces.DataAccess.Repositories
{
    public interface IPlayerRepository: IRepository<IPlayer>
    {
        IEnumerable<IPlayer> GetPlayersForServer(string serverId);
        IEnumerable<IPlayer> GetPlayersForCampaign(string campaignId);
    }
}
