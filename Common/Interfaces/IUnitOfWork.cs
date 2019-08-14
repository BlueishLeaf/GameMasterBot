using Common.Interfaces.Repositories;

namespace Common.Interfaces
{
    public interface IUnitOfWork
    {
        ICampaignRepository Campaigns { get; }
        IPlayerRepository Players { get; }
        ISessionRepository Sessions { get; }
    }
}
