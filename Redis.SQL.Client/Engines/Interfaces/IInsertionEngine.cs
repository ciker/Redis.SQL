using System.Threading.Tasks;

namespace Redis.SQL.Client.Engines.Interfaces
{
    internal interface IInsertionEngine
    {
        Task ExecuteInsertStatement(string statement);
        Task InsertEntity<TEntity>(TEntity entity);
    }
}