using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Interfaces.Entities.Core;

namespace Common.Interfaces.DataAccess.Repositories
{
    public interface ISessionRepository
    {
        IEnumerable<ISession> GetAllAfterDate(DateTime date);
        IEnumerable<ISession> GetForCampaign(ulong serverId, string campaignId);
        IEnumerable<ISession> GetForCampaignAfterDate(ulong serverId, string campaignId, DateTime date);
        IEnumerable<ISession> GetForCampaignForPeriod(ulong serverId, string campaignId, DateTime after, DateTime before);
        Task Add(ISession session);
        Task Update(ISession session);
        Task Remove(ISession session);
        Task RemoveRange(IEnumerable<ISession> sessions);
    }
}
