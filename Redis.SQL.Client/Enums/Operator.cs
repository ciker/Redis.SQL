namespace Redis.SQL.Client.Enums
{
    internal enum Operator : long
    {
        LessThan = 1,
        GreaterThan = 2,
        LessThanOrEqualTo = 4,
        GreaterThanOrEqualTo = 8,
        NotEqual = 16,
        Equals = 32
    }
}
