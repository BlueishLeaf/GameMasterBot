using System;
using System.Collections.Generic;
using Common.Interfaces.DataAccess.Repositories;
using Common.Interfaces.Entities.Core;

namespace DataAccess.Repositories
{
    public class SessionRepository: Repository<ISession>, ISessionRepository
    {
        public GameMasterContext GameMasterContext => Context as GameMasterContext;

        public SessionRepository(GameMasterContext context) : base(context) { }

        public IEnumerable<ISession> GetSessionsForCampaign(string campaignId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ISession> GetSessionsForServer(string serverId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ISession> GetSessionsForPlayer(string playerId)
        {
            throw new NotImplementedException();
        }
    }
}
