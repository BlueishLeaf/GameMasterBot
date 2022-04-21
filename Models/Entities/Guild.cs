using System.Collections.Generic;

namespace GameMasterBot.Models.Entities
{
    public class Guild
    {
        public long Id { get; set; }
        public ulong DiscordId { get; set; }
        public List<User> Users { get; set; } = new();
        public List<Campaign> Campaigns { get; set; } = new();
    }
}