using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Common.Interfaces.DataAccess.Repositories;
using Common.Interfaces.Entities.Core;
using Entities.Core;

namespace DataAccess.Repositories
{
    public class SessionRepository: Repository, ISessionRepository
    {
        public GameMasterContext GameMasterContext => Context as GameMasterContext;

        public SessionRepository(GameMasterContext context) : base(context) { }

        public IEnumerable<ISession> GetAllUpcoming() => 
            Context.QueryAsync<Session>(
                $"Session",
                QueryOperator.GreaterThanOrEqual,
                new[] { $"Session#{DateTime.UtcNow}" },
                new DynamoDBOperationConfig
                {
                    IndexName = "Entity-Sk-Index"
                })
                .GetNextSetAsync().Result;

        public IEnumerable<ISession> GetForCampaign(string campaignId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ISession> GetForServer(string serverId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ISession> GetForPlayer(string playerId)
        {
            throw new NotImplementedException();
        }

        public async Task Add(ISession session)
        {
            session.Pk = $"Campaign#{session.CampaignId}";
            session.Sk = $"Session#{session.Date}";
            session.Entity = "Session";
            session.Ts = DateTime.Now;
            await Context.SaveAsync(session as Session);
        }

        public async Task Remove(string campaignId, string sessionId)
        {
            throw new NotImplementedException();
        }
    }
}
