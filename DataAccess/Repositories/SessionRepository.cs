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
        public SessionRepository(DynamoDBContext context) : base(context) { }

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

        public IEnumerable<ISession> GetForCampaign(ulong serverId, string campaignId) =>
            Context.QueryAsync<Session>(
                    $"Campaign#{serverId}#{campaignId}",
                    QueryOperator.BeginsWith,
                    new[] {"Session#"})
                .GetNextSetAsync().Result;

        public IEnumerable<ISession> GetForCampaignAfterDate(ulong serverId, string campaignId, DateTime date) =>
            Context.QueryAsync<Session>(
                    $"Campaign#{serverId}#{campaignId}",
                    QueryOperator.GreaterThanOrEqual,
                    new[] { $"Session#{date:O}" })
                .GetNextSetAsync().Result;

        public IEnumerable<ISession> GetForCampaignForPeriod(ulong serverId, string campaignId, DateTime after, DateTime before) =>
            Context.QueryAsync<Session>(
                    $"Campaign#{serverId}#{campaignId}",
                    QueryOperator.Between,
                    new[] { $"Session#{after:O}", $"Session#{before:O}" })
                .GetNextSetAsync().Result;

        public async Task Add(ISession session)
        {
            session.Pk = $"Campaign#{session.ServerId}#{session.CampaignId}";
            session.Sk = $"Session#{session.Date:O}";
            session.Entity = "Session";
            session.Ts = DateTime.UtcNow;
            await Context.SaveAsync(session as Session);
        }

        public async Task Update(ISession session) => await Context.SaveAsync(session as Session);

        public async Task Remove(ISession session) => await Context.DeleteAsync(session as Session);

        public async Task RemoveRange(IEnumerable<ISession> sessions)
        {
            var batch = Context.CreateBatchWrite<Session>();
            foreach (var session in sessions) batch.AddDeleteItem((Session) session);
            await batch.ExecuteAsync();
        }
    }
}
