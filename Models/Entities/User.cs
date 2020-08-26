using System.Collections.Generic;

namespace GameMasterBot.Models.Entities
{
    public class User
    {
        public ulong Id { get; set; }
        public string Username { get; set; }
        public string? TimeZoneId { get; set; }
        public virtual List<GuildUser> GuildUsers { get; set; } = new List<GuildUser>();
        public virtual List<CampaignUser> CampaignUsers { get; set; } = new List<CampaignUser>();
    }
}