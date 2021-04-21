using System.Collections.Generic;

namespace GameMasterBot.Models.Entities
{
    public class Guild
    {
        public long Id { get; set; }
        public ulong DiscordId { get; set; }
        public string Name { get; set; } = null!;
        public virtual List<User> Users { get; set; } = new();
        public virtual List<Campaign> Campaigns { get; set; } = new();
    }
}