using System;
using System.ComponentModel.DataAnnotations;

namespace Redis.SQL.Client.Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var client = new RedisSqlClient();

            var u1 = new User
            {
                Name = "   test  white  space    ",
                Age = 12,
                Created = new DateTime(2017, 12, 12),
                Class = 'd',
                Id = 100,
                StartTime = TimeSpan.FromHours(23)
            };

            var u2 = new User
            {
                Name = "John",
                Age = 25,
                Created = DateTime.Now.AddHours(3),
                Class = 'a',
                Id = 101,
                StartTime = TimeSpan.FromHours(3)
            };

            var u3 = new User
            {
                Name = "Mark",
                Age = 30,
                Created = DateTime.Now.AddDays(1),
                Class = 'b',
                Id = 102,
                StartTime = TimeSpan.FromHours(10)
            };

            //client.ExecuteSql("create user( name:string,   age:int32, created:datetime, starttime:timespan, verified:boolean, class:char, id:int64)   ");

            //client.ExecuteSql("delete user where id =100");

            client.Update(u1);
            //client.Insert(u1);
            //client.Insert(u2);
            //client.Insert(u3);

            //var result = client.Query<User>(x => x.Id == 100);
            //result.ContinueWith(x =>
            //{
            //    var n = x.Result;
            //});

            //client.Create<User>();

            //client.ExecuteSql("insert user(name, age, created, starttime,   id, class) values (       '   r a my  '     , 40, '9/24/2017', '03:00:00', 110, 'a')");

            //client.ExecuteSql("update user set name     = '    rah   me=d,    '  , age=19   where id=110 ");

            Console.ReadLine();
        }
    }

    public class User
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public DateTime Created { get; set; }
        public TimeSpan StartTime { get; set; }
        [Key]
        public long Id { get; set; }
        public char Class { get; set; }
    }
}