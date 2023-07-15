using MongoDB.Driver;

namespace EDT.MongoProxy.Abstration;

public interface IMongoDbConnection
{
    IMongoClient DatabaseClient { get; }
    string DatabaseName { get; }
}
