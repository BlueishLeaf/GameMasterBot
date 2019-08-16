using System;
using System.Collections.Generic;
using Common.Interfaces.DataAccess.Repositories;
using Common.Interfaces.Entities.Core;

namespace DataAccess.Repositories
{
    public class PlayerRepository: Repository<IPlayer>, IPlayerRepository
    {
        public GameMasterContext GameMasterContext => Context as GameMasterContext;

        public PlayerRepository(GameMasterContext context) : base(context) { }

        public IEnumerable<IPlayer> GetPlayersForServer(string serverId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IPlayer> GetPlayersForCampaign(string campaignId)
        {
            throw new NotImplementedException();
        }
    }
}
