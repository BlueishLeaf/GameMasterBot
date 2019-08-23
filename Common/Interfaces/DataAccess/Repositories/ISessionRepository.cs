using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Interfaces.Entities.Core;

namespace Common.Interfaces.DataAccess.Repositories
{
    public interface ISessionRepository
    {
        IEnumerable<ISession> GetAllAfterDate(DateTime date);
        IEnumerable<ISession> GetForCampaign(string campaignId);
        IEnumerable<ISession> GetForPlayer(string playerId);
        Task Add(ISession session);
        Task Update(ISession session);
        Task Remove(ISession session);
    }
}
