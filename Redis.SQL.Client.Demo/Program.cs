﻿using System;

namespace Redis.SQL.Client.Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var client = new RedisSqlClient();

            client.ExecuteSql("update user set name='ahmed,' where id=123");


            Console.ReadLine();
        }
    }
}