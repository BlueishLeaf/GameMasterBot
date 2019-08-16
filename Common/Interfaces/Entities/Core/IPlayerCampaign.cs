namespace Common.Interfaces.Entities.Core
{
    public interface IPlayerCampaign
    {
        string Id { get; set; }
        string Name { get; set; }
        string System { get; set; }
        string GameMaster { get; set; }
    }
}
