using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Redis.SQL.Client.Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();

            Console.ReadLine();
        }

        private static async Task MainAsync()
        {
            var client = new RedisSqlClient();

            await client.Create<Employee>(); //Creating an entity type in the data store
            Console.WriteLine("Entity Type Created");
            
            var employees = GetEmployees().ToList();

            foreach (var employee in employees)
            {
                await client.Insert(employee); //Inserting data in the data store
            }

            Console.WriteLine($"{employees.Count} Entities Inserted");

            var rnd = new Random();

            int GetRandom() => rnd.Next(0, employees.Count - 1);

            var randomEmployee1 = employees[GetRandom()];
            var randomEmployee2 = employees[GetRandom()];
            var randomEmployee3 = employees[GetRandom()];

            //Warming Up
            await client.Query<Employee>(x => (x.Name == randomEmployee1.Name || x.Age >= randomEmployee2.Age) && x.Insured == !randomEmployee3.Insured && x.Department <= 'c');

            var timespan = TimeSpan.FromHours(5);

            //Querying data
            await TestQuery<Employee>(client, x => x.Name == randomEmployee3.Name || (x.Joined >= randomEmployee1.Joined && x.ShiftStartTime == timespan));
            await TestQuery<Employee>(client, x => (x.Age > randomEmployee3.Age && x.ShiftStartTime < timespan) || x.Name == randomEmployee2.Name);
            await TestQuery<Employee>(client, x => !x.Insured);
            await TestQuery<Employee>(client, x => (x.Joined < randomEmployee2.Joined && !x.Insured && x.Age < randomEmployee1.Age) || x.Id == randomEmployee3.Id);

            //Deleting an entity
            await client.Delete(randomEmployee1);

            //Updating an entity
            randomEmployee2.Name = Guid.NewGuid().ToString();
            await client.Update(randomEmployee2);
        }

        private static async Task TestQuery<T>(RedisSqlClient client, Expression<Func<T, bool>> expr)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var query = await client.Query(expr);
            stopwatch.Stop();
            Console.WriteLine($"Fetched {query.Count()} entities in {stopwatch.ElapsedMilliseconds} MS");
        }

        private static IEnumerable<Employee> GetEmployees()
        {
            var rnd = new Random();
            const string departments = "ABCDEF";

            var result = new List<Employee>();
            for (var i = 0; i < 1000; i++)
            {
                result.Add(new Employee
                {
                    Name = Guid.NewGuid().ToString(),
                    Age = rnd.Next(18, 70),
                    Department = departments[rnd.Next(0, departments.Length - 1)],
                    Id = i,
                    ShiftStartTime = TimeSpan.FromHours(rnd.Next(0, 24)),
                    Insured = i % 5 != 0,
                    Joined = new DateTime(rnd.Next(2000, 2018), rnd.Next(1, 13), rnd.Next(1, 29))
                });
            }
            return result;
        }
    }
}