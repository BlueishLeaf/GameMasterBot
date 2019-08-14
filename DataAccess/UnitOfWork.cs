using Amazon.DynamoDBv2.DataModel;
using Common.Interfaces;
using Common.Interfaces.Repositories;
using DataAccess.repositories;

namespace DataAccess
{
    public class UnitOfWork: IUnitOfWork
    {
        public ICampaignRepository Campaigns { get; }
        public IPlayerRepository Players { get; }
        public ISessionRepository Sessions { get; }

        public UnitOfWork(DynamoDBContext context)
        {
            Campaigns = new CampaignRepository(context);
            Players = new PlayerRepository(context);
            Sessions = new SessionRepository(context);
        }
    }
}
