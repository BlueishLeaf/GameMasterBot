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
    }
}
