namespace EdisonTalk.MongoProxy.Core;

public class UnitOfWork : IUnitOfWork
{
    private readonly IMongoDbContext _context;

    public UnitOfWork(IMongoDbContext context)
    {
        _context = context;
    }

    public bool SaveChanges(IClientSessionHandle session)
    {
        return _context.Commit(session) > 0;
    }

    public async Task<bool> SaveChangesAsync(IClientSessionHandle session)
    {
        return await _context.CommitAsync(session) > 0;
    }

    public IClientSessionHandle BeginTransaction()
    {
        return _context.StartSession();
    }

    public async Task<IClientSessionHandle> BeginTransactionAsync()
    {
        return await _context.StartSessionAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
