using System.Collections.Generic;

namespace Redis.SQL.Client.Models
{
    internal class UpdateModel : BaseModel
    {
        internal string WhereCondition { get; set; }
        internal IDictionary<string, string> UpdatedProperties { get; set; }
    }
}
