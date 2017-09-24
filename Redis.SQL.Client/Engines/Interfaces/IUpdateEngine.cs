using System.Threading.Tasks;

namespace Redis.SQL.Client.Engines.Interfaces
{
    internal interface IUpdateEngine
    {
        Task ExecuteUpdateStatement(string sql);
        Task UpdateEntity<TEntity>(TEntity entity);
    }
}
