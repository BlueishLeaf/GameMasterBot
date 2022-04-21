using System.Collections.Generic;

namespace GameMasterBot.Models.Entities
{
    public class User
    {
        public long Id { get; set; }
        public ulong DiscordId { get; set; }
        public string TimeZoneId { get; set; }
        public List<Guild> Guilds { get; set; } = new();
        public List<CampaignPlayer> Players { get; set; } = new();
        public List<GameMaster> GameMasters { get; set; } = new();
    }
}