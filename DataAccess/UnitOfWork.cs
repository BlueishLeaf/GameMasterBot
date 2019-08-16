using Common.Interfaces.DataAccess;
using Common.Interfaces.DataAccess.Repositories;
using DataAccess.Repositories;

namespace DataAccess
{
    public class UnitOfWork: IUnitOfWork
    {
        public ICampaignRepository Campaigns { get; }
        public IPlayerRepository Players { get; }
        public ISessionRepository Sessions { get; }

        public UnitOfWork(GameMasterContext context)
        {
            Campaigns = new CampaignRepository(context);
            Players = new PlayerRepository(context);
            Sessions = new SessionRepository(context);
        }
    }
}
