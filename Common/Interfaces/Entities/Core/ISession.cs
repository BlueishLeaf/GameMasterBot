using System;

namespace Common.Interfaces.Entities.Core
{
    public interface ISession: IDynamoDbItem
    {
        ulong ChannelId { get; }
        ulong ServerId { get; }
        string ServerName { get; }
        string CampaignId { get; }
        string CampaignName { get; }
        string Schedule { get; }
        DateTime Date { get; set; }
        bool ReminderSent { get; set; }
        bool TriggerSent { get; set; }
    }
}
