using System.Collections.Generic;
using Entities.core;

namespace Common.Interfaces.Repositories
{
    public interface ISessionRepository: IRepository<Session>
    {
        IEnumerable<Session> GetSessionsForCampaign(string campaignId);
        IEnumerable<Session> GetSessionsForServer(string serverId);
        IEnumerable<Session> GetSessionsForPlayer(string playerId);
    }
}
