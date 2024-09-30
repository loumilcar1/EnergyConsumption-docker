using System;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Transactions;

namespace ParserData
{
    public class DatabaseHandler
    {
        private readonly string _connectionString;

        public DatabaseHandler(string connectionString)
        {
            // Leer la cadena de conexión desde la configuración
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task InsertDataAsync((DateTime datetime, decimal value)[] datosSpain, (DateTime datetime, decimal value, int id_region)[] datosRegion)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Insertar datos nuevos en EnergyDemand_Spain
                if (datosSpain != null)
                {
                    foreach (var (datetime, value) in datosSpain)
                    {
                        string insertQuerySpain = @"
                    IF NOT EXISTS (SELECT 1 FROM EnergyDemand_Spain WHERE datetime = @datetime)
                    BEGIN
                        INSERT INTO EnergyDemand_Spain(datetime, value)
                        VALUES (@datetime, @value)
                    END";

                        using (var command = new SqlCommand(insertQuerySpain, connection))
                        {
                            command.Parameters.AddWithValue("@datetime", datetime);
                            command.Parameters.AddWithValue("@value", value);
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                }


                // Insertar datos nuevos en EnergyDemand_Region
                if (datosRegion != null)
                {
                    foreach (var (datetime, value, id_region) in datosRegion)
                    {
                        string insertQueryRegion = @"
                    IF NOT EXISTS (SELECT 1 FROM EnergyDemand_Region WHERE datetime = @datetime AND id_region = @id_region)
                    BEGIN
                        INSERT INTO EnergyDemand_Region(datetime, value, id_region)
                        VALUES (@datetime, @value, @id_region)
                    END";

                        using (var command = new SqlCommand(insertQueryRegion, connection))
                        {
                            command.Parameters.AddWithValue("@datetime", datetime);
                            command.Parameters.AddWithValue("@value", value);
                            command.Parameters.AddWithValue("@id_region", id_region);
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                }

                // Data inserted
                Console.WriteLine("\n");
                Console.WriteLine("3- Data inserted into database.");

            }
        }

        public async Task<DateTime> GetLastDateSpainAsync()
        {
            DateTime startDate;

            string query = "SELECT MAX(datetime) FROM EnergyDemand_Spain";

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand command = new SqlCommand(query, connection);
                    object result = await command.ExecuteScalarAsync();

                    if (result != DBNull.Value && result != null)
                    {
                        startDate = (DateTime)result;
                    }
                    else
                    {
                        throw new InvalidOperationException("Error: No data in the EnergyDemand_Spain table.");
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error: Failed to retrieve data from EnergyDemand_Spain.", ex);
            }

            return startDate;
        }

        public async Task<DateTime> GetLastDateRegionAsync(int regionId)
        {
            DateTime startDate;

            string query = "SELECT MAX(datetime) FROM EnergyDemand_Region WHERE id_region = @id_region";

            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@id_region", regionId);
                    object result = await command.ExecuteScalarAsync();

                    if (result != DBNull.Value && result != null)
                    {
                        startDate = (DateTime)result;
                    }
                    else
                    {
                        throw new InvalidOperationException($"Error: No data in the EnergyDemand_Region table for region ID: {regionId}.");
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new Exception("Error: Failed to retrieve data from EnergyDemand_Region.", ex);
            }

            return startDate;
        }
    }
}