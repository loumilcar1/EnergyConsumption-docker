using System;
using Microsoft.Extensions.Configuration;

namespace DatabaseInitializer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                // Configuración de la aplicación
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .Build();

                string? energyConsumptionDBConnectionString = configuration.GetConnectionString("EnergyConsumptionDB");
                string? masterConnectionString = configuration.GetConnectionString("MasterConnection");

                // Verificar si las cadenas de conexión son nulas
                if (string.IsNullOrEmpty(energyConsumptionDBConnectionString) || string.IsNullOrEmpty(masterConnectionString))
                {
                    Console.WriteLine("Error: The database connection string was not found in the configuration file.");
                    return;
                }

                // Inicializar la base de datos si no existe
                DatabaseInit dbInitializer = new DatabaseInit(configuration);
                await dbInitializer.InitializeDatabaseAsync();
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"Error: Configuration file not found. {ex.Message}");
                Environment.Exit(1);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: An unexpected error occurred: {ex.Message}");
                Environment.Exit(1);
            }
        }
    }
}
