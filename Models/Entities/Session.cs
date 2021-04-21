using System;
using GameMasterBot.Models.Enums;

namespace GameMasterBot.Models.Entities
{
    public class Session
    {
        public long Id { get; set; }
        public DateTime Timestamp { get; set; }
        public SessionState State { get; set; }
        public Schedule Schedule { get; set; }
        public long CampaignId { get; set; }
        public virtual Campaign Campaign { get; set; } = null!;
    }
}