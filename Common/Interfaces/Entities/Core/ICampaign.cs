namespace Common.Interfaces.Entities.Core
{
    public interface ICampaign: IDynamoDbItem
    {
        string Id { get; set; }
        string Name { get; set; }
        string GameMaster { get; set; }
        string CreatedBy { get; set; }
        string System { get; set; }
        string Url { get; set; }
    }
}
