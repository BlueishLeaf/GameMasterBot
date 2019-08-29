﻿using System;

namespace Common.Interfaces.Entities.Core
{
    public interface ISession: IDynamoDbItem
    {
        ulong ChannelId { get; set; }
        ulong ServerId { get; set; }
        string ServerName { get; set; }
        string CampaignId { get; set; }
        string CampaignName { get; set; }
        string Schedule { get; set; }
        DateTime Date { get; set; }
        bool ReminderSent { get; set; }
        bool TriggerSent { get; set; }
        long Expiry { get; set; } 
    }
}
