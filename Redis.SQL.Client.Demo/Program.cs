using System;

namespace Redis.SQL.Client.Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var client = new RedisSqlClient();

            client.Create<User>();

            //client.ExecuteSql("update user set name     = ' ah me=d,'  , age=30   where id=123 ");

            Console.ReadLine();
        }
    }

    public class User
    {
        public string Name { get; set; }
        public string Age { get; set; }
        public DateTime Created { get; set; }
        public TimeSpan StartTime { get; set; }
        public long Id { get; set; }
        public char Class { get; set; }
    }
}