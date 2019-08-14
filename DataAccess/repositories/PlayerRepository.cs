using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;
using Common.Interfaces.Repositories;
using Entities.core;

namespace DataAccess.repositories
{
    public class PlayerRepository: Repository<Player>, IPlayerRepository
    {
        public PlayerRepository(DynamoDBContext context) : base(context) { }

        public IEnumerable<Player> GetPlayersForServer(string serverId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Player> GetPlayersForCampaign(string campaignId)
        {
            throw new NotImplementedException();
        }
    }
}
