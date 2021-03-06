﻿using System;
using GameMasterBot.Models.Enums;

namespace GameMasterBot.Models.Entities
{
    public class Session
    {
        public ulong Id { get; set; }
        public DateTime Timestamp { get; set; }
        public SessionState State { get; set; }
        public Schedule Schedule { get; set; }
        public ulong CampaignId { get; set; }
        public virtual Campaign Campaign { get; set; } = null!;
    }
}