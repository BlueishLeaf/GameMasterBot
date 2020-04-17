using System.Collections.Generic;

namespace Common.Interfaces.Entities.Core
{
    public interface ICampaign: IDynamoDbItem
    {
        string Id { get; }
        string Name { get; }
        string GameMasterName { get; }
        ulong GameMasterId { get; }
        string CreatedBy { get; }
        string System { get; }
        string Url { get; }
        ulong ServerId { get; }
        List<string> Players { get; set; }
    }
}
