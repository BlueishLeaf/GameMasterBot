using System;
using System.Collections.Generic;
using System.Globalization;
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

        public IEnumerable<ISession> GetAllAfterDate(DateTime date) => 
            Context.QueryAsync<Session>(
                "Session",
                QueryOperator.GreaterThanOrEqual,
                new[] { $"Session#{date:O}" },
                new DynamoDBOperationConfig
                {
                    IndexName = "Entity-Sk-Index"
                })
                .GetNextSetAsync().Result;

        public IEnumerable<ISession> GetForCampaign(string campaignId) =>
            Context.QueryAsync<Session>(
                $"Campaign#{campaignId}",
                QueryOperator.BeginsWith,
                new[] {"Session#"})
                .GetNextSetAsync().Result;

        public IEnumerable<ISession> GetForPlayer(string playerId)
        {
            throw new NotImplementedException();
        }

        public async Task Add(ISession session)
        {
            session.Pk = $"Campaign#{session.CampaignId}";
            session.Sk = $"Session#{session.Date:O}";
            session.Entity = "Session";
            session.Ts = DateTime.UtcNow;
            await Context.SaveAsync(session as Session);
        }

        public async Task Update(ISession session) => await Context.SaveAsync(session as Session);

        public async Task Remove(ISession session) => await Context.DeleteAsync(session as Session);
    }
}
