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
            parser.ParseCondition("user", "name='john' or age=25");
            
            //parser.ParseCondition("user", "x=1 and z = 3 OR y = 2");

            //parser.ParseCondition("user", "x=1 and (y=2 or z=3)");
            //parser.ParseCondition("user", "(x =1) and (y = 2)");
            //parser.ParseCondition("user", "( x =   '1')and( x = 2 ) ");
            //parser.ParseCondition("user", "(x = 1 or y = 2) and z = 3");
            //parser.ParseCondition("user", @"((((((x = 'a or)())b' Or x > 2)) aNd ((orx < 5)))OR    (y=10))))");
            //parser.ParseCondition("user", "y=2 or z=3");
            //parser.ParseCondition("user", "(y=2 or z=3)");
            //parser.ParseCondition("user", "c = 'testandor'or( a = 1    and( ( (x     = 'a or ()) b'and y =    2  ) or z =  3)   ))");
            //parser.ParseCondition("user", "(x=1 and z = 3) OR y = 2");

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

            //var u4 = new User
            //{
            //    Name = "Andy",
            //    Age = 28,
            //    Created = DateTime.UtcNow.AddHours(4),
            //    StartTime = new TimeSpan(17, 30, 30),
            //    Verified = true
            //};

            //var u5 = new User
            //{
            //    Name = "Paul",
            //    Age = 30,
            //    Created = DateTime.UtcNow.AddHours(5),
            //    StartTime = new TimeSpan(02, 50, 50),
            //    Verified = false
            //};

            //var u6 = new User
            //{
            //    Name = "Peter",
            //    Age = 40,
            //    Created = DateTime.UtcNow.AddHours(6),
            //    StartTime = new TimeSpan(12, 10, 30),
            //    Verified = true
            //};

            //var u7 = new User
            //{
            //    Name = "Tony",
            //    Age = 30,
            //    Created = DateTime.UtcNow.AddHours(7),
            //    StartTime = new TimeSpan(05, 40, 45),
            //    Verified = true
            //};

            //var u8 = new User
            //{
            //    Name = "Ryan",
            //    Age = 27,
            //    Created = DateTime.UtcNow.AddHours(8),
            //    StartTime = new TimeSpan(10, 10, 30),
            //    Verified = true
            //};

            //var u9 = new User
            //{
            //    Name = "Wayne",
            //    Age = 25,
            //    Created = DateTime.UtcNow.AddHours(9),
            //    StartTime = new TimeSpan(13, 05, 50),
            //    Verified = true
            //};

            //var engine = new RedisSqlCreationEngine();
            //engine.CreateEntity(u1);
            //engine.CreateEntity(u2);
            //engine.CreateEntity(u3);
            //engine.CreateEntity(u4);
            //engine.CreateEntity(u5);
            //engine.CreateEntity(u6);
            //engine.CreateEntity(u7);
            //engine.CreateEntity(u8);
            //engine.CreateEntity(u9);


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