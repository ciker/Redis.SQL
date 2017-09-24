using System.Collections.Generic;
using System.Threading.Tasks;

namespace Redis.SQL.Client.Engines.Interfaces
{
    internal interface IQueryEngine
    {
        Task<IEnumerable<TEntity>> QueryEntities<TEntity>(string condition);
        Task<IEnumerable<dynamic>> QueryEntities(string entityName, string condition);
        Task<IEnumerable<string>> GetEntityKeys(string entityName, string condition);
        Task<TEntity> GetEntityByKey<TEntity>(string key);
        Task<string> ConstructWhereStatementFromIdentifiers<TEntity>(TEntity entity);
    }
}
