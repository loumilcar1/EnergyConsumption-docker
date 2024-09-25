using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Connector
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                // Configuración para leer el appsettings.json
                // Cargar la configuración desde appsettings.json
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

                // Imprimir solo la hora en formato HH:mm:ss
                Console.WriteLine("Start: " + DateTime.Now);

                // Crear la instancia de CreateOrUpdateCSV
                CreateOrUpdateCSV csvHandler = new CreateOrUpdateCSV(config);

                // Obtener la cadena de conexión
                string connectionString = config.GetConnectionString("EnergyConsumptionDB");

                // Pasar esa instancia a DatabaseHandler
                DatabaseHandler databaseHandler = new DatabaseHandler(connectionString, csvHandler);

                // Fetch data from EnergyDemand_Spain
                List<CSVData> records = await databaseHandler.FetchDataAsync();

                // Fetch data from EnergyConsumption_Region
                List<CSVDataRegion> regionRecords = await databaseHandler.FetchDataRegionAsync();


                if (records.Any())
                {

                    // Create or update the CSV file for EnergyDemand_Spain
                    csvHandler.UpdateCsv(records);

                }
                else
                {
                    Console.WriteLine("\nNo new data to export for EnergyDemand_Spain.");
                }


                if (regionRecords.Any())
                {
                    // Create or update the CSV file for EnergyConsumption_Region
                    csvHandler.UpdateCsvRegion(regionRecords);
                }
                else
                {
                    Console.WriteLine("\nNo new data to export for EnergyConsumption_Region.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error exporting data to CSV: " + ex.Message);
            }
        }
    }
}
