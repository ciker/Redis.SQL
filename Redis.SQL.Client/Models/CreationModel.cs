using System.Collections.Generic;

namespace Redis.SQL.Client.Models
{
    internal class CreationModel : BaseModel
    {
        internal string EntityName { get; set; }

        internal IDictionary<string, string> Properties { get; set; }
    }
}
