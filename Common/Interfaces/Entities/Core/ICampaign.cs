using System.Collections.Generic;

namespace Common.Interfaces.Entities.Core
{
    public interface ICampaign: IDynamoDbItem
    {
        string Id { get; set; }
        string Name { get; set; }
        string GameMasterName { get; set; }
        ulong GameMasterId { get; set; }
        string CreatedBy { get; set; }
        string System { get; set; }
        string Url { get; set; }
        string ServerName { get; set; }
        ulong ServerId { get; set; }
        List<string> Players { get; set; }
    }
}
