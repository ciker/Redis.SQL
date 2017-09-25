using System;
using System.Collections.Generic;
using System.Linq;
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

            await client.Create<Employee>();

            var employees = GetEmployees().ToList();

            foreach (var employee in employees)
            {
                await client.Insert(employee);
            }

            var rnd = new Random();

            var randomEmployee = employees[rnd.Next(0, employees.Count - 1)];
            var warmUpQuery = await client.Query<Employee>(x => x.Name == randomEmployee.Name);
        }


        private static IEnumerable<Employee> GetEmployees()
        {
            var rnd = new Random();
            const string departments = "ABCDEF";

            var result = new List<Employee>();
            for (var i = 0; i < 100; i++)
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