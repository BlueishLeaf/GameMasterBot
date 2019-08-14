using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;
using Common.Interfaces.Repositories;
using Entities.core;

namespace DataAccess.repositories
{
    public class SessionRepository: Repository<Session>, ISessionRepository
    {
        public SessionRepository(DynamoDBContext context) : base(context) { }

        public IEnumerable<Session> GetSessionsForCampaign(string campaignId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Session> GetSessionsForServer(string serverId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Session> GetSessionsForPlayer(string playerId)
        {
            throw new NotImplementedException();
        }
    }
}
