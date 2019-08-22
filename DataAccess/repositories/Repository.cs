using System;
using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;
using Common.Interfaces.DataAccess.Repositories;

namespace DataAccess.Repositories
{
    public class Repository
    {
        protected DynamoDBContext Context;

        public Repository(DynamoDBContext context) => Context = context;
    }
}
