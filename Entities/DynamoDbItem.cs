using System;
using Amazon.DynamoDBv2.DataModel;
using Common.Interfaces.Entities;

namespace Entities
{
    [DynamoDBTable("GameMasterBotTbl")]
    public class DynamoDbItem: IDynamoDbItem
    {
        [DynamoDBHashKey] public string Pk { get; set; }
        [DynamoDBRangeKey] public string Sk { get; set; }
        public DateTime Ts { get; set; }
    }
}
