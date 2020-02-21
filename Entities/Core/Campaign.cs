using System.Collections.Generic;
using Common.Interfaces.Entities.Core;

namespace Entities.Core
{
    public class Campaign: DynamoDbItem, ICampaign
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string GameMasterName { get; set; }
        public ulong GameMasterId { get; set; }
        public string CreatedBy { get; set; }
        public string System { get; set; }
        public string Url { get; set; }
        public string ServerName { get; set; }
        public ulong ServerId { get; set; }
        public List<string> Players { get; set; }
    }
}
