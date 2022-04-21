using System;
using System.Collections.Generic;

namespace GameMasterBot.Models.Entities
{
    public class Campaign
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string System { get; set; }
        public string Url { get; set; }
        public DateTime CreatedAt { get; set; }
        public ulong TextChannelId { get; set; }
        public ulong VoiceChannelId { get; set; }
        public ulong PlayerRoleId { get; set; }
        public ulong GameMasterRoleId { get; set; }
        public long GuildId { get; set; }
        public virtual Guild Guild { get; set; }
        public GameMaster GameMaster { get; set; }
        public List<CampaignPlayer> Players { get; } = new();
        public List<Session> Sessions { get; } = new();
    }
}