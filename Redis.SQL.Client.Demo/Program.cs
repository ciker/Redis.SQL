using System;
using System.Diagnostics;
using Redis.SQL.Client.Engines;

namespace Redis.SQL.Client.Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var client = new RedisSqlClient();

            //client.ExecuteWhere("user", @"name='john'");

            //client.ExecuteWhere("user", @"((age> 28 and class = 'b' ) or name = 'ahmed') or id=12778899");

            //client.ExecuteWhere("user", @"(name = 'ahmed' or (age> 28 and class = 'b' )) or id=12778899");

            client.ExecuteWhere("user", @"(Created > '09/12/2017 21:40 pm' and class != 'a') or (id =12213822 and verified = true)");
            //client.ExecuteWhere("user", @"Created > '1/30/1991'");
            //client.ExecuteWhere("user", @"starttime>='13:00:00'");
            //client.ExecuteWhere("user", @"Created > '1/30/1991 13:30:45'");

            //client.ExecuteWhere("user", @"age > 101");

            var u1 = new User
            {
                Name = "Ahmed",
                Age = 25,
                Created = DateTime.UtcNow,
                StartTime = new TimeSpan(10, 30, 30),
                Verified = true,
                Class = 'c',
                Id = 12218831
            };

            var u2 = new User
            {
                Name = "John",
                Age = 35,
                Created = DateTime.UtcNow.AddHours(1),
                StartTime = new TimeSpan(03, 45, 0),
                Verified = true,
                Class = 'a',
                Id = 12213822
            };

            var u3 = new User
            {
                Name = "Mark",
                Age = 25,
                Created = DateTime.UtcNow.AddHours(3),
                StartTime = new TimeSpan(15, 0, 0),
                Verified = false,
                Class = 'b',
                Id = 12255551
            };

            var u4 = new User
            {
                Name = "Andy",
                Age = 28,
                Created = DateTime.UtcNow.AddHours(4),
                StartTime = new TimeSpan(17, 30, 30),
                Verified = true,
                Class = 'b',
                Id = 12778899
            };

            var u5 = new User
            {
                Name = "Paul",
                Age = 30,
                Created = DateTime.UtcNow.AddHours(5),
                StartTime = new TimeSpan(02, 50, 50),
                Verified = false,
                Class = 'a',
                Id = 12222221
            };

            var u6 = new User
            {
                Name = "Peter",
                Age = 40,
                Created = DateTime.UtcNow.AddHours(6),
                StartTime = new TimeSpan(12, 10, 30),
                Verified = true,
                Class = 'a',
                Id = 12201181
            };

            var u7 = new User
            {
                Name = "Tony",
                Age = 30,
                Created = DateTime.UtcNow.AddHours(7),
                StartTime = new TimeSpan(05, 40, 45),
                Verified = true,
                Class = 'b',
                Id = 12999831
            };

            var u8 = new User
            {
                Name = "Ryan",
                Age = 27,
                Created = DateTime.UtcNow.AddHours(8),
                StartTime = new TimeSpan(10, 10, 30),
                Verified = true,
                Class = 'b',
                Id = 12219011
            };

            var u9 = new User
            {
                Name = "Wayne",
                Age = 25,
                Created = DateTime.UtcNow.AddHours(9),
                StartTime = new TimeSpan(13, 05, 50),
                Verified = true,
                Class = 'c',
                Id = 12349131
            };

            var engine = new RedisSqlCreationEngine();
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
        public char Class { get; set; }
        public long Id { get; set; }
    }
}