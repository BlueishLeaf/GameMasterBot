namespace GameMasterBot.Models.Entities
{
    public class GuildUser
    {
        public ulong GuildId { get; set; }
        public virtual Guild Guild { get; set; } = null!;
        public ulong UserId { get; set; }
        public virtual User User { get; set; } = null!;
    }
}