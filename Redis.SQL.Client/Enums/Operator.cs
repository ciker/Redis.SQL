namespace Redis.SQL.Client.Enums
{
    internal enum Operator : long
    {
        GreaterThanOrEqualTo = 1,
        LessThanOrEqualTo = 2,
        GreaterThan = 4,
        LessThan = 8,
        NotEqual = 16,
        Equals = 32
    }
}
