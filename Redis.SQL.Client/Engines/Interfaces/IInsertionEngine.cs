using System.Collections.Generic;
using System.Threading.Tasks;

namespace Redis.SQL.Client.Engines.Interfaces
{
    internal interface IInsertionEngine
    {
        Task ExecuteInsertStatement(string statement);
        Task InsertEntity<TEntity>(TEntity entity);
        Task AddPropertyToStore(string entityName, string identifier, string propertyTypeName, string propertyName, string propertyValue);
        Task<string> EncodeEntity(string entityName, string identifier, IDictionary<string, string> propertyValues);
    }
}