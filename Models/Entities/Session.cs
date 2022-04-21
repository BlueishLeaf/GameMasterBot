using System;
using GameMasterBot.Models.Enums;

namespace GameMasterBot.Models.Entities
{
    public class Session
    {
        public long Id { get; set; }
        public DateTime Timestamp { get; set; }
        public SessionState State { get; set; }
        public ScheduleFrequency Frequency { get; set; }
        public long CampaignId { get; set; }
        public Campaign Campaign { get; set; }
    }
}