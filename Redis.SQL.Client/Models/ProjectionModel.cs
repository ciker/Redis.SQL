using System.Collections.Generic;

namespace Redis.SQL.Client.Models
{
    internal class ProjectionModel : BaseModel
    {
        internal string Query { get; set; }
        internal ICollection<string> ProjectedProperties { get; set; }
        internal string EntityName { get; set; }
        internal bool ProjectAllProperties { get; set; }
    }
}
