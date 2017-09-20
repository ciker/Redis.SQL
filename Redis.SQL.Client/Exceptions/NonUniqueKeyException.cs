using System;

namespace Redis.SQL.Client.Exceptions
{
    public class NonUniqueKeyException : Exception
    {
        public NonUniqueKeyException() : base("The Key Attribute was placed on a non-unique set of properties")
        {
            
        }
    }
}
