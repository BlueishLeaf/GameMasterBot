namespace GameMasterBot.Models.Entities
{
    public class CampaignUser
    {
        public ulong CampaignId { get; set; }
        public virtual Campaign Campaign { get; set; }
        public ulong UserId { get; set; }
        public virtual User User { get; set; }

        public override string ToString()
        {
            return User.Username;
        }
    }
}