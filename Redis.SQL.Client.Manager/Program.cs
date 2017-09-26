using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Redis.SQL.Client.Manager
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
            Console.WriteLine("Type -h for help\n");

            while (true)
            {
                Console.Write("> ");
                var input = Console.ReadLine();
                if (string.Equals(input?.Trim(), "-h", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Welcome to Redis.SQL Manager. Use Redis.SQL Manager to manage your Redis data store using SQL-like statements." +
                                      "\n\n-To create a new entity type use the format:\n\ncreate ENTITY (property:type)\n\nExample:\n\ncreate user (name:string, id:int64)" +
                                      "\n\n-Currently Supported types in Redis.SQL are: \n\n-string\n-int32\n-int64\n-char\n-boolean\n-datetime\n-timespan" +
                                      "\n\n-To insert a new entity type use the format:\n\ninsert ENTITY (property) values (value)\n\nExample:\n\ninsert user (name, id) values ('Ahmed', 1)" +
                                      "\n\n-To project entities from your data store, use the format: \n\nselect [*/comma separated properties] from ENTITY where property [=/!=/>=/<=/</>] value [and/or] ...\n\nExample:\n\nselect * from user where name='ahmed' or (age=30 and verified = true)" +
                                      "\n\n-To delete an entity from your data store, use the format: \n\ndelete ENTITY where [condition], note that providing the where condition is not obligatory.\n\nExample:\n\ndelete from user where id = 1" +
                                      "\n\n-To update an entity use the format: \n\nupdate ENTITY set property=value where [condition], note that providing the where condition is not obligatory.\n\nExample:\n\nupdate user set name='John', age=40 where id = 3\n\n");
                }
                else
                {
                    IList<IDictionary<string, string>> result;
                    try
                    {
                        result = (await client.ExecuteSql(input)).ToList();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"\n{e.Message}\n");
                        continue;
                    }

                    Console.WriteLine("\nExecuted Successfully\n");

                    var counter = 0;
                    foreach (var item in result)
                    {
                        Console.WriteLine($"Row #{++counter}");
                        foreach (var property in item)
                        {
                            Console.WriteLine($"-{property.Key}: {property.Value}");
                        }
                        Console.WriteLine("\n");
                    }
                }
            }
        }
    }
}