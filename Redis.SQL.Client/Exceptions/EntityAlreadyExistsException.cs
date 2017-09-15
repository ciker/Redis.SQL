using System;

namespace Redis.SQL.Client.Exceptions
{
    public class EntityAlreadyExistsException : Exception
    {
        internal EntityAlreadyExistsException(string entity) : base($"Cannot create entity: {entity} as it already exists in the data store")
        {
            
        }
    }
}