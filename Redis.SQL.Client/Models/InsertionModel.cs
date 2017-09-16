using System.Collections.Generic;

namespace Redis.SQL.Client.Models
{
    internal class InsertionModel : BaseModel
    {
        internal string EntityName { get; set; }
        internal IDictionary<string, string> PropertyValues { get; set; }
    }
}