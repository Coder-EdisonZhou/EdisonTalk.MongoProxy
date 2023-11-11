using MongoDB.Driver;
using System.Linq.Expressions;

namespace EdisonTalk.MongoProxy.Abstration;

public interface IMongoRepositoryBase<TEntity> where TEntity : MongoEntityBase, new()
{
    #region Create Part

    Task AddAsync(TEntity entity, IClientSessionHandle session = null);
    Task AddManyAsync(IEnumerable<TEntity> entityList, IClientSessionHandle session = null);

    #endregion

    #region Delete Part

    Task DeleteAsync(string id, IClientSessionHandle session = null);
    Task DeleteAsync(Expression<Func<TEntity, bool>> expression, IClientSessionHandle session = null);
    Task<DeleteResult> DeleteManyAsync(FilterDefinition<TEntity> filter, IClientSessionHandle session = null);
    Task<DeleteResult> DeleteManyAsync(Expression<Func<TEntity, bool>> expression, IClientSessionHandle session = null);

    #endregion

    #region Update Part

    Task UpdateAsync(TEntity entity, IClientSessionHandle session = null);
    Task UpdateAsync(Expression<Func<TEntity, bool>> expression, Expression<Action<TEntity>> entity, IClientSessionHandle session = null);
    Task UpdateAsync(FilterDefinition<TEntity> filter, UpdateDefinition<TEntity> update, IClientSessionHandle session = null);
    Task UpdateManyAsync(Expression<Func<TEntity, bool>> expression, UpdateDefinition<TEntity> update, IClientSessionHandle session = null);
    Task<UpdateResult> UpdateManayAsync(Dictionary<string, string> dict, FilterDefinition<TEntity> filter, IClientSessionHandle session = null);

    #endregion

    #region Read Part

    Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> expression, bool readFromPrimary = true);
    Task<TEntity> GetAsync(string id, bool readFromPrimary = true);
    Task<IEnumerable<TEntity>> GetAllAsync(bool readFromPrimary = true);
    Task<long> CountAsync(Expression<Func<TEntity, bool>> expression, bool readFromPrimary = true);
    Task<long> CountAsync(FilterDefinition<TEntity> filter, bool readFromPrimary = true);
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, bool readFromPrimary = true);
    Task<List<TEntity>> FindListAsync(FilterDefinition<TEntity> filter, string[]? field = null, SortDefinition<TEntity>? sort = null, bool readFromPrimary = true);
    Task<List<TEntity>> FindListByPageAsync(FilterDefinition<TEntity> filter, int pageIndex, int pageSize, string[]? field = null, SortDefinition<TEntity>? sort = null, bool readFromPrimary = true);

    #endregion
}
