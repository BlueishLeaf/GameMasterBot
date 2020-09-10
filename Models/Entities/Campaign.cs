using System.Collections.Generic;

namespace GameMasterBot.Models.Entities
{
    public class Campaign
    {
        public ulong Id { get; set; }
        public string Name { get; set; } = null!;
        public string System { get; set; } = null!;
        public string? Url { get; set; }
        public ulong TextChannelId { get; set; }
        public ulong VoiceChannelId { get; set; }
        public ulong PlayerRoleId { get; set; }
        public ulong GameMasterRoleId { get; set; }
        public ulong GuildId { get; set; }
        public virtual Guild Guild { get; set; } = null!;
        public ulong UserId { get; set; }
        public virtual User User { get; set; } = null!;
        public virtual List<CampaignUser> CampaignUsers { get; } = new List<CampaignUser>();
        public virtual List<Session> Sessions { get; } = new List<Session>();
    }
}