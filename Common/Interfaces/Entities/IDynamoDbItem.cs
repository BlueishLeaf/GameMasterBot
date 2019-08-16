using System;

namespace Common.Interfaces.Entities
{
    public interface IDynamoDbItem
    {
        string Pk { get; set; }
        string Sk { get; set; }
        DateTime Ts { get; set; }
    }
}
