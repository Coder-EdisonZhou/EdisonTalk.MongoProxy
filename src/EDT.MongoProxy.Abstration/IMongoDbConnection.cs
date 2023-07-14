using MongoDB.Driver;

namespace EDT.MongoProxy.Abstration;

public interface IMongoDbConnection
{
    MongoClient DatabaseClient { get; }
    string DatabaseName { get; }
}
