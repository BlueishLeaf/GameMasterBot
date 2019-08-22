using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Interfaces.Entities.Core;

namespace Common.Interfaces.DataAccess.Repositories
{
    public interface ISessionRepository
    {
        IEnumerable<ISession> GetAllUpcoming();
        IEnumerable<ISession> GetForCampaign(string campaignId);
        IEnumerable<ISession> GetForServer(string serverId);
        IEnumerable<ISession> GetForPlayer(string playerId);
        Task Add(ISession session);
        Task Remove(string campaignId, string sessionId);
    }
}
