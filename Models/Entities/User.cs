using System.Collections.Generic;

namespace GameMasterBot.Models.Entities
{
    public class User
    {
        public long Id { get; set; }
        public ulong DiscordId { get; set; }
        public string Username { get; set; } = null!; // TODO: Shouldn't keep this in db as it can become out of date
        public string? TimeZoneId { get; set; }
        public virtual List<Guild> Guilds { get; set; } = new();
        public virtual List<Player> Players { get; set; } = new();
        public virtual List<GameMaster> GameMasters { get; set; } = new();
    }
}