using System.Collections.Generic;

namespace GameMasterBot.Models.Entities
{
    public class Guild
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public virtual List<GuildUser> GuildUsers { get; set; } = new List<GuildUser>();
    }
}