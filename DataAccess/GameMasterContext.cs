using System;
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
                new BasicAWSCredentials(
                    Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID"),
                    Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS")), 
                RegionEndpoint.EUWest1)) { }
    }
}
