namespace EdisonTalk.MongoProxy.Core;

public class MongoDbContext : IMongoDbContext
{
    private readonly IMongoDatabase _database;
    private readonly IMongoClient _mongoClient;
    private readonly IList<Func<IClientSessionHandle, Task>> _commands
        = new List<Func<IClientSessionHandle, Task>>();

    public MongoDbContext(IMongoDbConnection dbClient)
    {
        _mongoClient = dbClient.DatabaseClient;
        _database = _mongoClient.GetDatabase(dbClient.DatabaseName);
    }

    public void AddCommand(Func<IClientSessionHandle, Task> func)
    {
        _commands.Add(func);
    }

    public async Task AddCommandAsync(Func<IClientSessionHandle, Task> func)
    {
        _commands.Add(func);
        await Task.CompletedTask;
    }

    /// <summary>
    /// NOTES: Only works in Cluster mode
    /// </summary>
    public int Commit(IClientSessionHandle session)
    {
        try
        {
            session.StartTransaction();

            foreach (var command in _commands)
            {
                command(session);
            }

            session.CommitTransaction();
            return _commands.Count;
        }
        catch (Exception ex)
        {
            session.AbortTransaction();
            return 0;
        }
        finally
        {
            _commands.Clear();
        }
    }

    /// <summary>
    /// NOTES: Only works in Cluster mode
    /// </summary>
    public async Task<int> CommitAsync(IClientSessionHandle session)
    {
        try
        {
            session.StartTransaction();

            foreach (var command in _commands)
            {
                await command(session);
            }

            await session.CommitTransactionAsync();
            return _commands.Count;
        }
        catch (Exception ex)
        {
            await session.AbortTransactionAsync();
            return 0;
        }
        finally
        {
            _commands.Clear();
        }
    }

    public IClientSessionHandle StartSession()
    {
        var session = _mongoClient.StartSession();
        return session;
    }

    public async Task<IClientSessionHandle> StartSessionAsync()
    {
        var session = await _mongoClient.StartSessionAsync();
        return session;
    }

    public IMongoCollection<T> GetCollection<T>(string name)
    {
        return _database.GetCollection<T>(name);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
