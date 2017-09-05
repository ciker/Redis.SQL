using System;
using System.Collections.Generic;
using Redis.SQL.Client.Parsers;

namespace Redis.SQL.Client.Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var parser = new ConditionalParser();

            var clauses = new List<string>();
            var operators = new List<string>();
            //parser.ParseCondition(@"((x==1)OR y == 2)", clauses, operators);
            parser.ParseCondition(@"((((((x == 'a or)())b' Or x > 2)) aNd ((orx < 5)))OR    (y==10))))", clauses, operators);
            Console.ReadLine();
        }

    }

    public class TestType
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public DateTime Created { get; set; }
        public TimeSpan Time { get; set; }
        public bool Vaccinated { get; set; }
    }
}