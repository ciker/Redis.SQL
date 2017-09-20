using System;

namespace Redis.SQL.Client.Exceptions
{
    public class KeyAttributeMissingException : Exception
    {

        public KeyAttributeMissingException() : base("Key Attribute missing. The Key Attribute should be added to your entity's unique properties/fields.")
        {
            
        }
    }
}