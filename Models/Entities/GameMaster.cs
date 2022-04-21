namespace GameMasterBot.Models.Entities
{
    public class GameMaster
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public User User { get; set; }
        public long CampaignId { get; set; }
        public Campaign Campaign { get; set; }
    }
}