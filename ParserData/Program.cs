using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ParserData
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                // Configurar la lectura de la configuración desde appsettings.json
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                // Imprimir solo la hora en formato HH:mm:ss
                Console.WriteLine("Start: " + DateTime.Now);
                Console.WriteLine("\n");
                Fetcher fetcher = new Fetcher(config);
                Parser parser = new Parser();
                DatabaseHandler databaseManager = new DatabaseHandler(config);

                // 1- Fetch data
                var (jsonSpain, jsonRegion) = await fetcher.FetchDataAsync();

                // Check if jsonSpain or jsonRegion are null
                if (jsonSpain != null || jsonRegion != null)
                {
                    // 2- Parse JSON data
                    var (dataSpain, dataRegion) = Parser.ParserData(jsonSpain, jsonRegion);

                    // 3- Insert data into database
                    await databaseManager.InsertDataAsync(dataSpain, dataRegion);
                    Console.WriteLine("\n");
                }
                else
                {
                    Console.WriteLine("No data fetched. Data is already up to date.");
                    Console.WriteLine("\n");
                }
                // Espera a que el usuario presione Enter para cerrar la consola
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred: {0}", e.Message);
            }
        }
    }
}