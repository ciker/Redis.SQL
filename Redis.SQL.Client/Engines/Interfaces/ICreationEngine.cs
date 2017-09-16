using System.Threading.Tasks;

namespace Redis.SQL.Client.Engines.Interfaces
{
    public interface ICreationEngine
    {
        Task CreateEntity<TEntity>();
        Task ExecuteCreateStatement(string createStatement);
    }
}
