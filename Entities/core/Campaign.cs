namespace Entities.core
{
    public class Campaign: DynamoDbItem
    {
        public string Name { get; set; }
        public string System { get; set; }
    }
}
