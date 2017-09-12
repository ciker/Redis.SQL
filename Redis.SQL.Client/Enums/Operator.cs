namespace Redis.SQL.Client.Enums
{
    internal enum Operator : long
    {
        LessThanOrEqualTo = 1,
        GreaterThanOrEqualTo = 2,
        LessThan = 4,
        GreaterThan = 8,
        NotEqual = 16,
        Equals = 32
    }
}
