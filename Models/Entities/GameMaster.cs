namespace GameMasterBot.Models.Entities
{
    public class GameMaster
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public virtual User User { get; set; } = null!;
        public long CampaignId { get; set; }
        public virtual Campaign Campaign { get; set; } = null!;
    }
}