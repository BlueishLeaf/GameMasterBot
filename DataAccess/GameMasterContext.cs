using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime;

namespace DataAccess
{
    public class GameMasterContext: DynamoDBContext
    {
        public GameMasterContext(): base(
            new AmazonDynamoDBClient(
                new BasicAWSCredentials(Environment.GetEnvironmentVariable("aws-access-token"), Environment.GetEnvironmentVariable("aws-secret")),
                RegionEndpoint.EUWest1)) { }
    }
}
