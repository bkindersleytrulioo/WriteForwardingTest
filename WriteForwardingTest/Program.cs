// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;
using WriteForwardingTest;

var config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", false, true)
#if DEBUG
    .AddUserSecrets<Program>()
#endif
    .Build();
var appSettingsConfig = config
    .GetSection("AppSettings");

var consistencySetting = appSettingsConfig["AuroraReadConsistency"];
Console.WriteLine($"Read consistency setting is {consistencySetting}");

var connectionString = appSettingsConfig["ConnectionString"];




while (true)
{
    Console.WriteLine("\r\n||||||||||||||||||||||||||||||||");
    Console.WriteLine("Enter 's' to create schema\r\nEnter 'r' to read last 10 entries\r\nEnter 'w1' to write an entry in one statement\r\nEnter 'w2' to write an entry in two statement\r\nCtrl+C to exit");
    Console.WriteLine("||||||||||||||||||||||||||||||||\r\n");

    var entry = Console.ReadLine();
    switch (entry)
    {
        case "s":
            var creator = new SchemaCreator(connectionString);
            creator.CreateSchema();
            break;
        case "r":
            var readAccess = new DataAccess(consistencySetting, connectionString);
            var data = readAccess.GetData();
            Console.WriteLine(string.Join("\r\n", data.Select(d => $"{d.ID} | {d.Key}")));
            break;
        case "w1":
            var writeAccess1 = new DataAccess(consistencySetting, connectionString);
            writeAccess1.SaveDataAndGetIDInOneStatement($"{Guid.NewGuid()}-{DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
            break;
        case "w2":
            var writeAccess2 = new DataAccess(consistencySetting, connectionString);
            writeAccess2.SaveDataAndGetIDInTwoStatements($"{Guid.NewGuid()}-{DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm:ss.fff")}");
            break;
        default:
            Console.WriteLine("Not a recognized option.");
            break;
    }
}
