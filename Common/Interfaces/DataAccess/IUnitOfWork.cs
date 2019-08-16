using Common.Interfaces.DataAccess.Repositories;

namespace Common.Interfaces.DataAccess
{
    public interface IUnitOfWork
    {
        ICampaignRepository Campaigns { get; }
        IPlayerRepository Players { get; }
        ISessionRepository Sessions { get; }
    }
}
