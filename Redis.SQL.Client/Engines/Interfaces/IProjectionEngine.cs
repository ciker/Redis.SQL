using System.Collections.Generic;
using System.Threading.Tasks;

namespace Redis.SQL.Client.Engines.Interfaces
{
    internal interface IProjectionEngine
    {
        Task<IEnumerable<IDictionary<string, string>>> ExecuteSelectStatement(string sql);
    }
}
