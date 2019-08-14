using System;

namespace Entities.core
{
    public class Session: DynamoDbItem
    {
        public DateTime StartTime { get; set; }
    }
}
