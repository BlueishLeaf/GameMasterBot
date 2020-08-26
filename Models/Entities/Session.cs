using System;
using GameMasterBot.Models.Enums;

namespace GameMasterBot.Models.Entities
{
    public class Session
    {
        public ulong Id { get; set; }
        public DateTime Timestamp { get; set; }
        public bool ReminderSent { get; set; }
        public bool Activated { get; set; }
        public Schedule Schedule { get; set; }
        public ulong CampaignId { get; set; }
        public virtual Campaign Campaign { get; set; }
    }
}