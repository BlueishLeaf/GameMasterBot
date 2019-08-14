using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;
using Common.Interfaces.Repositories;
using Entities.core;

namespace DataAccess.repositories
{
    public class CampaignRepository: Repository<Campaign>, ICampaignRepository
    {
        public CampaignRepository(DynamoDBContext context) : base(context) { }

        public IEnumerable<Campaign> GetCampaignsForServer(string serverId)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Campaign> GetCampaignsForPlayer(string playerId)
        {
            throw new NotImplementedException();
        }
    }
}
