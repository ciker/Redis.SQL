using System;

namespace Redis.SQL.Client.Exceptions
{
    public class EntityNotFoundException : Exception
    {
        internal EntityNotFoundException(string entity) : base ($"Cannot insert the given value as {entity} is not found in the data store")
        {
            
        }
    }
}
