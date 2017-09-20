using System.Collections.Generic;

namespace Redis.SQL.Client.Models
{
    internal class CreationModel : BaseModel
    {
        internal IDictionary<string, string> Properties { get; set; }
    }
}
