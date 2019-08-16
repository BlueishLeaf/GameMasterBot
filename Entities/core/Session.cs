using System;
using Common.Interfaces.Entities.Core;

namespace Entities.Core
{
    public class Session: DynamoDbItem, ISession
    {
        public string Id { get; set; }
        public DateTime StartTime { get; set; }
    }
}
