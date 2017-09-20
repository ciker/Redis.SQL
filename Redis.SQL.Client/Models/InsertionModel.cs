using System.Collections.Generic;

namespace Redis.SQL.Client.Models
{
    internal class InsertionModel : BaseModel
    {
        internal IDictionary<string, string> PropertyValues { get; set; }
    }
}