using System;

namespace Common.Interfaces.Entities
{
    public interface IDynamoDbItem
    {
        string Pk { set; }
        string Sk { set; }
        string Entity { set; }
        DateTime Ts { set; }
    }
}
