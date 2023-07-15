using EDT.MongoProxy.Utils;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;

namespace EDT.MongoProxy.Core;

public class MongoRepositoryBase<TEntity> : IMongoRepositoryBase<TEntity>
        where TEntity : MongoEntityBase, new()
{
    protected readonly IMongoDbContext _dbContext;
    protected readonly IMongoCollection<TEntity> _dbSet;
    private readonly string _collectionName;
    private const string _keyField = "_id";

    public MongoRepositoryBase(IMongoDbContext mongoDbContext)
    {
        _dbContext = mongoDbContext;
        _collectionName = typeof(TEntity).GetAttributeValue((TableAttribute m) => m.Name)
            ?? typeof(TEntity).Name;
        if (string.IsNullOrWhiteSpace(_collectionName))
            throw new ArgumentNullException("Mongo DatabaseName can't be NULL! Please set the attribute Table in your entity class.");

        _dbSet = mongoDbContext.GetCollection<TEntity>(_collectionName);
    }

    #region Create Part

    public async Task AddAsync(TEntity entity, IClientSessionHandle session = null)
    {
        if (session == null)
            await _dbSet.InsertOneAsync(entity);
        else
            await _dbContext.AddCommandAsync(async (session) => await _dbSet.InsertOneAsync(entity));
    }

    public async Task AddManyAsync(IEnumerable<TEntity> entityList, IClientSessionHandle session = null)
    {
        if (session == null)
            await _dbSet.InsertManyAsync(entityList);
        else
            await _dbContext.AddCommandAsync(async (session) => await _dbSet.InsertManyAsync(entityList));
    }

    #endregion

    # region Delete Part

    public async Task DeleteAsync(string id, IClientSessionHandle session = null)
    {
        if (session == null)
            await _dbSet.DeleteOneAsync(Builders<TEntity>.Filter.Eq(_keyField, new ObjectId(id)));
        else
            await _dbContext.AddCommandAsync(async (session) => await _dbSet.DeleteOneAsync(Builders<TEntity>.Filter.Eq(_keyField, new ObjectId(id))));
    }

    public async Task DeleteAsync(Expression<Func<TEntity, bool>> expression, IClientSessionHandle session = null)
    {
        if (session == null)
            await _dbSet.DeleteOneAsync(expression);
        else
            await _dbContext.AddCommandAsync(async (session) => await _dbSet.DeleteOneAsync(expression));
    }

    public async Task<DeleteResult> DeleteManyAsync(FilterDefinition<TEntity> filter, IClientSessionHandle session = null)
    {
        if (session == null)
            return await _dbSet.DeleteManyAsync(filter);

        await _dbContext.AddCommandAsync(async (session) => await _dbSet.DeleteManyAsync(filter));
        return new DeleteResult.Acknowledged(10);
    }

    public async Task<DeleteResult> DeleteManyAsync(Expression<Func<TEntity, bool>> expression, IClientSessionHandle session = null)
    {
        if (session == null)
            return await _dbSet.DeleteManyAsync(expression);

        await _dbContext.AddCommandAsync(async (session) => await _dbSet.DeleteManyAsync(expression));
        return new DeleteResult.Acknowledged(10);
    }

    #endregion

    #region Update Part

    public async Task UpdateAsync(TEntity entity, IClientSessionHandle session = null)
    {
        if (session == null)
            await _dbSet.ReplaceOneAsync(item => item.Id == entity.Id, entity);
        else
            await _dbContext.AddCommandAsync(async (session) => await _dbSet.ReplaceOneAsync(item => item.Id == entity.Id, entity));
    }

    public async Task UpdateAsync(Expression<Func<TEntity, bool>> expression, Expression<Action<TEntity>> entity, IClientSessionHandle session = null)
    {
        var fieldList = new List<UpdateDefinition<TEntity>>();

        if (entity.Body is MemberInitExpression param)
        {
            foreach (var item in param.Bindings)
            {
                var propertyName = item.Member.Name;
                object propertyValue = null;

                if (item is not MemberAssignment memberAssignment) continue;

                if (memberAssignment.Expression.NodeType == ExpressionType.Constant)
                {
                    if (memberAssignment.Expression is ConstantExpression constantExpression)
                        propertyValue = constantExpression.Value;
                }
                else
                {
                    propertyValue = Expression.Lambda(memberAssignment.Expression, null).Compile().DynamicInvoke();
                }

                if (propertyName != _keyField)
                {
                    fieldList.Add(Builders<TEntity>.Update.Set(propertyName, propertyValue));
                }
            }
        }

        if (session == null)
            await _dbSet.UpdateOneAsync(expression, Builders<TEntity>.Update.Combine(fieldList));
        else
            await _dbContext.AddCommandAsync(async (session) => await _dbSet.UpdateOneAsync(expression, Builders<TEntity>.Update.Combine(fieldList)));
    }

    public async Task UpdateAsync(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update, IClientSessionHandle session = null)
    {
        if (session == null)
            await _dbSet.UpdateOneAsync(filter, update);
        else
            await _dbContext.AddCommandAsync(async (session) => await _dbSet.UpdateOneAsync(filter, update));
    }

    public async Task UpdateManyAsync(Expression<Func<TEntity, bool>> expression, UpdateDefinition<TEntity> update, IClientSessionHandle session = null)
    {
        if (session == null)
            await _dbSet.UpdateManyAsync(expression, update);
        else
            await _dbContext.AddCommandAsync(async (session) => await _dbSet.UpdateManyAsync(expression, update));
    }

    public async Task<UpdateResult> UpdateManayAsync(Dictionary<string, string> dic, FilterDefinition<TEntity> filter, IClientSessionHandle session = null)
    {
        var t = new TEntity();
        // Fields to be updated
        var list = new List<UpdateDefinition<TEntity>>();
        foreach (var item in t.GetType().GetProperties())
        {
            if (!dic.ContainsKey(item.Name)) continue;
            var value = dic[item.Name];
            list.Add(Builders<TEntity>.Update.Set(item.Name, value));
        }
        var updatefilter = Builders<TEntity>.Update.Combine(list);

        if (session == null)
            return await _dbSet.UpdateManyAsync(filter, updatefilter);

        await _dbContext.AddCommandAsync(async (session) => await _dbSet.UpdateManyAsync(filter, updatefilter));
        return new UpdateResult.Acknowledged(10, 10, null);
    }

    #endregion

    #region Read Part

    public async Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> expression, bool readFromPrimary = true)
    {
        var readPreference = GetReadPreference(readFromPrimary);
        var queryData = await _dbSet.WithReadPreference(readPreference)
             .Find(expression)
             .FirstOrDefaultAsync();
        return queryData;
    }

    public async Task<TEntity> GetAsync(string id, bool readFromPrimary = true)
    {
        var readPreference = GetReadPreference(readFromPrimary);
        var queryData = await _dbSet.WithReadPreference(readPreference).FindAsync(Builders<TEntity>.Filter.Eq(_keyField, new ObjectId(id)));
        return queryData.FirstOrDefault();
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync(bool readFromPrimary = true)
    {
        var readPreference = GetReadPreference(readFromPrimary);
        var queryAllData = await _dbSet.WithReadPreference(readPreference).FindAsync(Builders<TEntity>.Filter.Empty);
        return queryAllData.ToList();
    }

    public async Task<long> CountAsync(Expression<Func<TEntity, bool>> expression, bool readFromPrimary = true)
    {
        var readPreference = GetReadPreference(readFromPrimary);
        return await _dbSet.WithReadPreference(readPreference).CountDocumentsAsync(expression);
    }

    public async Task<long> CountAsync(FilterDefinition<TEntity> filter, bool readFromPrimary = true)
    {
        var readPreference = GetReadPreference(readFromPrimary);
        return await _dbSet.WithReadPreference(readPreference).CountDocumentsAsync(filter);
    }

    public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, bool readFromPrimary = true)
    {
        var readPreference = GetReadPreference(readFromPrimary);
        return await Task.FromResult(_dbSet.WithReadPreference(readPreference).AsQueryable().Any(predicate));
    }

    public async Task<List<TEntity>> FindListAsync(FilterDefinition<TEntity> filter, string[]? field = null, SortDefinition<TEntity>? sort = null, bool readFromPrimary = true)
    {
        var readPreference = GetReadPreference(readFromPrimary);
        if (field == null || field.Length == 0)
        {
            if (sort == null)
                return await _dbSet.WithReadPreference(readPreference).Find(filter).ToListAsync();

            return await _dbSet.WithReadPreference(readPreference).Find(filter).Sort(sort).ToListAsync();
        }

        var fieldList = new List<ProjectionDefinition<TEntity>>();
        for (int i = 0; i < field.Length; i++)
        {
            fieldList.Add(Builders<TEntity>.Projection.Include(field[i].ToString()));
        }
        var projection = Builders<TEntity>.Projection.Combine(fieldList);
        fieldList?.Clear();

        if (sort == null)
            return await _dbSet.WithReadPreference(readPreference).Find(filter).Project<TEntity>(projection).ToListAsync();

        return await _dbSet.WithReadPreference(readPreference).Find(filter).Sort(sort).Project<TEntity>(projection).ToListAsync();
    }

    public async Task<List<TEntity>> FindListByPageAsync(FilterDefinition<TEntity> filter, int pageIndex, int pageSize, string[]? field = null, SortDefinition<TEntity>? sort = null, bool readFromPrimary = true)
    {
        var readPreference = GetReadPreference(readFromPrimary);
        if (field == null || field.Length == 0)
        {
            if (sort == null)
                return await _dbSet.WithReadPreference(readPreference).Find(filter).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToListAsync();

            return await _dbSet.WithReadPreference(readPreference).Find(filter).Sort(sort).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToListAsync();
        }

        var fieldList = new List<ProjectionDefinition<TEntity>>();
        for (int i = 0; i < field.Length; i++)
        {
            fieldList.Add(Builders<TEntity>.Projection.Include(field[i].ToString()));
        }
        var projection = Builders<TEntity>.Projection.Combine(fieldList);
        fieldList?.Clear();

        if (sort == null)
            return await _dbSet.WithReadPreference(readPreference).Find(filter).Project<TEntity>(projection).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToListAsync();

        return await _dbSet.WithReadPreference(readPreference).Find(filter).Sort(sort).Project<TEntity>(projection).Skip((pageIndex - 1) * pageSize).Limit(pageSize).ToListAsync();
    }

    #endregion

    #region Protected Methods

    protected ReadPreference GetReadPreference(bool readFromPrimary)
    {
        if (readFromPrimary)
            return ReadPreference.PrimaryPreferred;
        else
            return ReadPreference.SecondaryPreferred;
    }

    #endregion
}
