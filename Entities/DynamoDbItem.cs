using System;
using Amazon.DynamoDBv2.DataModel;
using Common.Interfaces.Entities;

namespace Entities
{
    [DynamoDBTable("GameMasterBotTbl")]
    public class DynamoDbItem: IDynamoDbItem
    {
        [DynamoDBHashKey]
        public string Pk { get; set; }
        [DynamoDBRangeKey, DynamoDBGlobalSecondaryIndexRangeKey("Entity-Sk-Index")]
        public string Sk { get; set; }
        [DynamoDBGlobalSecondaryIndexHashKey("Entity-Sk-Index")]
        public string Entity { get; set; }
        public DateTime Ts { get; set; }
    }
}
