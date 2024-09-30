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
                // Leer la configuración desde appsettings.json
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                // Obtener la cadena de conexión
                string connectionString = config.GetConnectionString("EnergyConsumptionDB");

                // Imprimir hora en formato HH:mm:ss
                Console.WriteLine("Start: " + DateTime.Now);
                Console.WriteLine("\n");

                // Crear la instancia de Fetcher
                Fetcher fetcher = new Fetcher(config);

                // Crear la instancia de Parser
                Parser parser = new Parser();

                // Crear la instancia de DatabaseHandler
                DatabaseHandler databaseManager = new DatabaseHandler(connectionString);

                // 1- Fetch: Obtener datos
                var (jsonSpain, jsonRegion) = await fetcher.FetchDataAsync();

                // Verificar si se han obtenido datos de la API
                if (jsonSpain != null || jsonRegion != null)
                {
                    // 2- Parse: Transformar datos
                    var (dataSpain, dataRegion) = Parser.ParserData(jsonSpain, jsonRegion);

                    // 3- DatabaseHandler: Almacenar datos 
                    await databaseManager.InsertDataAsync(dataSpain, dataRegion);
                    Console.WriteLine("\n");
                }
                else
                {
                    Console.WriteLine("No data fetched. Data is already up to date.");
                    Console.WriteLine("\n");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
                Environment.Exit(1);
            }
        }
    }
}