using Amazon.DynamoDBv2.DataModel;

namespace DataAccess.Repositories
{
    public class Repository
    {
        protected readonly DynamoDBContext Context;

        protected Repository(DynamoDBContext context) => Context = context;
    }
}
