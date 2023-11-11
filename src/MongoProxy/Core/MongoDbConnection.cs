using EdisonTalk.MongoProxy.Configurations;

namespace EdisonTalk.MongoProxy.Core;

public class MongoDbConnection : IMongoDbConnection
{
    public IMongoClient DatabaseClient { get; }
    public string DatabaseName { get; }

    public MongoDbConnection(MongoDatabaseConfigs configs, IConfiguration configuration)
    {
        DatabaseClient = new MongoClient(configs.GetMongoClientSettings(configuration));
        DatabaseName = configs.DatabaseName;
    }
}
