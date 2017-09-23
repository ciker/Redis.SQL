using System;

namespace Redis.SQL.Client.Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var client = new RedisSqlClient();

            client.ExecuteSql("update user set name     = ' ah med,'   where id=123 ");
            //client.ExecuteSql("insert users (name    ,   age) values('a,     b' ,             30)");


            Console.ReadLine();
        }
    }
}