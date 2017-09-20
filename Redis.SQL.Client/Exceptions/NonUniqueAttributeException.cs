using System;

namespace Redis.SQL.Client.Exceptions
{
    public class NonUniqueAttributeException : Exception
    {
        public NonUniqueAttributeException() : base("The UniqueIdentifier Attribute was placed on a non-unique set of properties")
        {
            
        }
    }
}
