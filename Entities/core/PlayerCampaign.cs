using Common.Interfaces.Entities.Core;

namespace Entities.Core
{
    public class PlayerCampaign: DynamoDbItem, IPlayerCampaign
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string System { get; set; }
        public string GameMaster { get; set; }
    }
}
