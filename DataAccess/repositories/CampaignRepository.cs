﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DocumentModel;
using Common.Interfaces.DataAccess.Repositories;
using Common.Interfaces.Entities.Core;
using Entities;
using Entities.Core;

namespace DataAccess.Repositories
{
    public class CampaignRepository: Repository, ICampaignRepository
    {
        public GameMasterContext GameMasterContext => Context as GameMasterContext;

        public CampaignRepository(GameMasterContext context) : base(context) { }

        public async Task<ICampaign> Get(string serverId, string campaignId) => await Context.LoadAsync<Campaign>($"Server#{serverId}", $"Campaign#{campaignId}");

        public IEnumerable<ICampaign> GetForServer(string serverId) => Context
            .QueryAsync<Campaign>(
                $"Server#{serverId}",
                QueryOperator.BeginsWith,
                new[] { "Campaign#" })
            .GetNextSetAsync().Result;

        public IEnumerable<ICampaign> GetForPlayer(string serverId, string playerName) => Context
            // TODO: Add filters
            .QueryAsync<Campaign>(
                $"Server#{serverId}",
                QueryOperator.BeginsWith,
                new[] { "Campaign#" })
            .GetNextSetAsync().Result;

        public void Add(ICampaign campaign)
        {
            // Complete the campaign object
            campaign.Pk = $"Server#{campaign.ServerId}";
            campaign.Sk = $"Campaign#{campaign.Id}";
            campaign.Ts = DateTime.Now;
            campaign.Entity = "Campaign";
            // Save the campaign object
            Context.SaveAsync(campaign as Campaign);
        }

        public void Remove(string serverId, string campaignId) =>
            Context.DeleteAsync(new DynamoDbItem
            {
                Pk = $"Server#{serverId}",
                Sk = $"Campaign#{campaignId}"
            });
    }
}
