using DbUp;
using System;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace Garaaz.DbScripts
{
    class Program
    {
        static int Main(string[] args)
        {
            // Ref - https://dbup.readthedocs.io/en/latest/

            // Run this project to create database, user defined functions and stored procedure

            var connectionString =
                args.FirstOrDefault()
                ?? ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

            // Create database if not exists
            EnsureDatabase.For.SqlDatabase(connectionString);

            var upgrader =
                DeployChanges.To
                    .SqlDatabase(connectionString)
                    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                    .LogToConsole()
                    .Build();

            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(result.Error);
                Console.ResetColor();

                Console.ReadLine();
             
                return -1;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Success!");
            Console.ResetColor();
            return 0;
        }
    }
}
