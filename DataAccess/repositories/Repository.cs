using Amazon.DynamoDBv2.DataModel;

namespace DataAccess.Repositories
{
    public class Repository
    {
        protected DynamoDBContext Context;

        public Repository(DynamoDBContext context) => Context = context;
    }
}
