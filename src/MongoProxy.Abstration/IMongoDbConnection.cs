using MongoDB.Driver;

namespace EdisonTalk.MongoProxy.Abstration;

public interface IMongoDbConnection
{
    IMongoClient DatabaseClient { get; }
    string DatabaseName { get; }
}
