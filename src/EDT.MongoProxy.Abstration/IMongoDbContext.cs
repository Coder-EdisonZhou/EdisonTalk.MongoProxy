using MongoDB.Driver;

namespace EDT.MongoProxy.Abstration;

public interface IMongoDbContext : IDisposable
{
    void AddCommand(Func<IClientSessionHandle, Task> func);
    Task AddCommandAsync(Func<IClientSessionHandle, Task> func);
    int Commit(IClientSessionHandle session);
    Task<int> CommitAsync(IClientSessionHandle session);
    IClientSessionHandle StartSession();
    Task<IClientSessionHandle> StartSessionAsync();
    IMongoCollection<T> GetCollection<T>(string name);
}
