using System.Collections.Generic;
using System.Threading.Tasks;

namespace Redis.SQL.Client.Engines.Interfaces
{
    internal interface IQueryEngine
    {
        Task<IEnumerable<TEntity>> QueryEntities<TEntity>(string condition);
        Task<IEnumerable<dynamic>> QueryEntities(string entityName, string condition);
    }
}
