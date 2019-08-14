using System;
using Amazon.DynamoDBv2.DataModel;

namespace Entities
{
    [DynamoDBTable("GameMasterBotTbl")]
    public class DynamoDbItem
    {
        [DynamoDBHashKey] public string Pk { get; set; }
        [DynamoDBRangeKey] public string Sk { get; set; }
        public DateTime Ts { get; set; }
    }
}
