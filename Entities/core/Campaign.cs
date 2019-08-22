using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;
using Common.Interfaces.Entities.Core;

namespace Entities.Core
{
    public class Campaign: DynamoDbItem, ICampaign
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string GameMaster { get; set; }
        public string CreatedBy { get; set; }
        public string System { get; set; }
        public string Url { get; set; }
        public string ServerName { get; set; }
        public string ServerId { get; set; }
        public List<string> Players { get; set; }
        [DynamoDBIgnore] public ISession[] Sessions { get; set; }
    }
}
