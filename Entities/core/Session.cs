using System;
using Common.Interfaces.Entities.Core;

namespace Entities.Core
{
    public class Session: DynamoDbItem, ISession
    {
        public ulong ChannelId { get; set; }
        public string CampaignId { get; set; }
        public string Schedule { get; set; }
        public DateTime Date { get; set; }
    }
}
