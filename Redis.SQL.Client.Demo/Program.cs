using System;

namespace Redis.SQL.Client.Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var client = new RedisSqlEngine();

            var entity = new TestType
            {
                Age = 30,
                Name = "Ahmed",
                Created = DateTime.UtcNow
            };

            client.CreateEntity(entity);
            Console.ReadLine();
        }

    }

    public class TestType
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public DateTime Created { get; set; }
    }
}