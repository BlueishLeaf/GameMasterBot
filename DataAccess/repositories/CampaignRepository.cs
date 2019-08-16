using System.Collections.Generic;
using Amazon.DynamoDBv2.DocumentModel;
using Common.Interfaces.DataAccess.Repositories;
using Common.Interfaces.Entities.Core;
using Entities.Core;

namespace DataAccess.Repositories
{
    public class CampaignRepository: Repository<ICampaign>, ICampaignRepository
    {
        public GameMasterContext GameMasterContext => Context as GameMasterContext;

        public CampaignRepository(GameMasterContext context) : base(context) { }

        public IEnumerable<ICampaign> GetCampaignsForServer(string serverId) => Context.QueryAsync<Campaign>(
                $"Server#{serverId}",
                QueryOperator.BeginsWith,
                new[] { "Campaign#" })
            .GetNextSetAsync().Result;

        public IEnumerable<IPlayerCampaign> GetCampaignsForPlayer(string playerId) => Context
            .QueryAsync<PlayerCampaign>(
                $"Player#{playerId}",
                QueryOperator.BeginsWith,
                new[] { "Campaign#" })
            .GetNextSetAsync().Result;
    }
}
