using System;

namespace Redis.SQL.Client.Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var client = new RedisSqlClient();

            //var creationDate = DateTime.UtcNow;
            //var age = 25;
            //var classValue = 'b';
            //var name = "ahmed";
            //var time = TimeSpan.FromHours(3);

            //var result = client.Query<User>(x => x.StartTime == TimeSpan.Zero);
            //var result2 = client.Query<User>(user => user.Class == classValue && user.StartTime == time);
            //var result = client.Query<User>(user => (user.Age == age && user.Name == name) || user.Created == creationDate);

            var result = client.ExecuteSql("   SeLeCT user.id, name    frOm  user    whEre(id=12201181 or( class='c' and age  <  28)  ) ");
            //var result = client.ExecuteSql("  CreAte user(NAME    :    STRING,   age:int32,  created:datetime,starttime:timespan,verified:boolean,     class:char,  id:int64  )  ");
            //var result = client.ExecuteSql("  insert   user   (name,    age,created,starttime,    verified ,  class , id) values('Test Insert1'  , 25, '12/26/2017 10:00', '09:30:00'   , false, 'c', 1202100)  ");

            //var result = client.Query<User>(x => x.Age == 25 || x.Id == 12999831);
            result.ContinueWith(x =>
            {
                var res = x.Result;

                //foreach (var item in res)
                //{
                //    foreach (var prop in item)
                //    {
                //        Console.WriteLine($"{prop.Key}: {prop.Value}");
                //    }
                //}
            });

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

            //client.Create<User>();
            //client.Insert(u1);
            //client.Insert(u2);
            //client.Insert(u3);
            //client.Insert(u4);
            //client.Insert(u5);
            //client.Insert(u6);
            //client.Insert(u7);
            //client.Insert(u8);
            //client.Insert(u9);

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