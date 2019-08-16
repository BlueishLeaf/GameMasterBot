﻿using Common.Interfaces.Entities.Core;

namespace Entities.Core
{
    public class Player: DynamoDbItem, IPlayer
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
