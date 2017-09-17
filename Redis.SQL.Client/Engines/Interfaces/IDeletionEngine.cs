using System.Threading.Tasks;

namespace Redis.SQL.Client.Engines.Interfaces
{
    internal interface IDeletionEngine
    {
        Task ExecuteDeleteStatement(string statement);
    }
}
