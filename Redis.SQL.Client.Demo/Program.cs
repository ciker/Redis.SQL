using System;
using System.Collections.Generic;
using Redis.SQL.Client.Engines;
using Redis.SQL.Client.Parsers;

namespace Redis.SQL.Client.Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var parser = new ConditionalParser();
            parser.ParseCondition(@"name !=     'ahmed'");


            //parser.ParseCondition(@"((x==1)OR y == 2)", clauses, operators);
            //parser.ParseCondition(@"((((((x == 'a or)())b' Or x > 2)) aNd ((orx < 5)))OR    (y==10))))");


            //var u1 = new User
            //{
            //    Name = "Ahmed",
            //    Age = 25,
            //    Created = DateTime.UtcNow,
            //    StartTime = new TimeSpan(10, 30, 30),
            //    Verified = true
            //};

            //var u2 = new User
            //{
            //    Name = "John",
            //    Age = 35,
            //    Created = DateTime.UtcNow.AddHours(1),
            //    StartTime = new TimeSpan(03, 45, 0),
            //    Verified = true
            //};

            //var u3 = new User
            //{
            //    Name = "Mark",
            //    Age = 25,
            //    Created = DateTime.UtcNow.AddHours(3),
            //    StartTime = new TimeSpan(15, 0, 0),
            //    Verified = false
            //};

            //var engine = new RedisSqlCreationEngine();
            //engine.CreateEntity(u1);
            //engine.CreateEntity(u2);
            //engine.CreateEntity(u3);


            Console.ReadLine();
        }

    }

    public class User
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public DateTime Created { get; set; }
        public TimeSpan StartTime { get; set; }
        public bool Verified { get; set; }
    }
}