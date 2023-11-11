using MongoDB.Driver;

namespace EdisonTalk.MongoProxy.Abstration;

public interface IUnitOfWork : IDisposable
{
    IClientSessionHandle BeginTransaction();
    Task<IClientSessionHandle> BeginTransactionAsync();
    bool SaveChanges(IClientSessionHandle session);
    Task<bool> SaveChangesAsync(IClientSessionHandle session);
}
