using System;

namespace Redis.SQL.Client.Exceptions
{
    public class UniqueIdentifierMissingException : Exception
    {

        public UniqueIdentifierMissingException() : base("UniqueIdentifier Attribute missing. The UniqueIdentifier Attribute should be added to your entity's unique properties.")
        {
            
        }
    }
}