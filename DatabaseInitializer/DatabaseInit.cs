using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace DatabaseInitializer
{
    public class DatabaseInit
    {
        private readonly string _masterConnectionString;
        private readonly string _connectionString;

        public DatabaseInit(IConfiguration configuration)
        {
            // Validar que las cadenas de conexión no sean nulas
            _masterConnectionString = configuration.GetConnectionString("MasterConnection")
                ?? throw new ArgumentNullException(nameof(configuration), "Master connection string cannot be null.");
            _connectionString = configuration.GetConnectionString("EnergyConsumptionDB")
                ?? throw new ArgumentNullException(nameof(configuration), "Energy consumption connection string cannot be null.");
        }

        public async Task InitializeDatabaseAsync()
        {
            Console.WriteLine("Checking if the database exists...");

            // Verificar si la base de datos existe
            bool exists = await Task.Run(() => DatabaseExists("EnergyConsumption"));

            if (!exists)
            {
                Console.WriteLine("The database does not exist. Creating database and tables...");

                // Ejecutar script para crear la base de datos
                string createDbScriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts", "EnergyConsumptionDB.sql");
                await Task.Run(() => ExecuteSqlScript(_masterConnectionString, createDbScriptPath));

                // Ejecutar script para crear las tablas y rellenar datos
                string tablesScriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts", "TablesEnergyConsumption.sql");
                await Task.Run(() => ExecuteSqlScript(_connectionString, tablesScriptPath));

                Console.WriteLine("Database and tables created successfully.");
            }
            else
            {
                Console.WriteLine("The database already exists.");
            }
        }

        // Método para verificar si la base de datos existe
        private bool DatabaseExists(string databaseName)
        {
            using (SqlConnection connection = new SqlConnection(_masterConnectionString))
            {
                string query = "SELECT database_id FROM sys.databases WHERE Name = @databaseName";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@databaseName", databaseName);
                    connection.Open();
                    var result = command.ExecuteScalar();
                    return result != null;
                }
            }
        }

        // Método para ejecutar un script SQL desde un archivo
        private void ExecuteSqlScript(string connectionString, string scriptPath)
        {
            if (File.Exists(scriptPath))
            {
                // Leer el contenido del archivo de script
                string script = File.ReadAllText(scriptPath);

                // Dividir el script en partes usando "GO" como separador
                string[] scriptParts = script.Split(new[] { "GO" }, StringSplitOptions.RemoveEmptyEntries);

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    foreach (string scriptPart in scriptParts)
                    {
                        // Ejecutar cada parte del script
                        using (SqlCommand command = new SqlCommand(scriptPart, connection))
                        {
                            try
                            {
                                command.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error executing the following part of the script: \n{scriptPart}\nError: {ex.Message}");
                            }
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine($"The script file {scriptPath} was not found.");
            }
        }
    }
}
