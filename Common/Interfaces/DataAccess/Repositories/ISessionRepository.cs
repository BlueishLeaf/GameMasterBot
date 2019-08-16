using System.Collections.Generic;
using Common.Interfaces.Entities.Core;

namespace Common.Interfaces.DataAccess.Repositories
{
    public interface ISessionRepository: IRepository<ISession>
    {
        IEnumerable<ISession> GetSessionsForCampaign(string campaignId);
        IEnumerable<ISession> GetSessionsForServer(string serverId);
        IEnumerable<ISession> GetSessionsForPlayer(string playerId);
    }
}
