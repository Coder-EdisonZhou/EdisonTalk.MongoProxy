namespace EDT.MongoProxy.Core
{
    public class MongoDbConnection : IMongoDbConnection
    {
        public MongoClient DatabaseClient { get; }
        public string DatabaseName { get; }

        public MongoDbConnection(MongoDatabaseConfigs configs, IConfiguration configuration)
        {
            DatabaseClient = new MongoClient(configs.GetMongoClientSettings(configuration));
            DatabaseName = configs.DatabaseName;
        }
    }
}
