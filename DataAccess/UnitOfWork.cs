using Amazon.DynamoDBv2.DataModel;
using Common.Interfaces.DataAccess;
using Common.Interfaces.DataAccess.Repositories;
using DataAccess.Repositories;

namespace DataAccess
{
    public class UnitOfWork: IUnitOfWork
    {
        public ICampaignRepository Campaigns { get; }
        public ISessionRepository Sessions { get; }

        public UnitOfWork(GameMasterContext context)
        {
            Campaigns = new CampaignRepository(context);
            Sessions = new SessionRepository(context);
        }
    }
}
