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
                Created = DateTime.UtcNow,
                Time = new TimeSpan(10, 30, 2),
                Vaccinated = false
            };

            var entity2 = new TestType
            {
                Age = 20,
                Name = "Ramy",
                Created = DateTime.UtcNow.AddHours(1),
                Time = new TimeSpan(03, 10, 32),
                Vaccinated = true
            };

            client.CreateEntity(entity);
            client.CreateEntity(entity2);
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