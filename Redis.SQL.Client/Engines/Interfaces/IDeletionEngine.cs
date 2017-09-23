using System.Threading.Tasks;

namespace Redis.SQL.Client.Engines.Interfaces
{
    internal interface IDeletionEngine
    {
        Task ExecuteDeleteStatement(string statement);
        Task DeleteEntity<TEntity>(TEntity entity);
        Task PurgeProperty(string entityName, string entityKey, string propertyName, string propertyValue, string propertyTypeName);
    }
}
