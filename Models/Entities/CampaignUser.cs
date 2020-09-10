namespace GameMasterBot.Models.Entities
{
    public class CampaignUser
    {
        public ulong CampaignId { get; set; }
        public virtual Campaign Campaign { get; set; } = null!;
        public ulong UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public override string ToString()
        {
            return User.Username;
        }
    }
}