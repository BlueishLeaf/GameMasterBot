using Common.Interfaces.DataAccess.Repositories;

namespace Common.Interfaces.DataAccess
{
    public interface IUnitOfWork
    {
        ICampaignRepository Campaigns { get; }
        ISessionRepository Sessions { get; }
    }
}
