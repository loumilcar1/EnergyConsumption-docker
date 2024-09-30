using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Connector
{
    class DatabaseHandler
    {
        private readonly string _connectionString;
        private readonly CreateOrUpdateCSV _csvHandler;

        public DatabaseHandler(string connectionString, CreateOrUpdateCSV csvHandler)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _csvHandler = csvHandler ?? throw new ArgumentNullException(nameof(csvHandler));
        }

        public async Task<List<CSVData>> FetchDataAsync()
        {
            // Obtener la última fecha exportada desde los CSV
            DateTime? lastExportedDate = _csvHandler.GetLastExportedDate();

            // Si no hay una última fecha exportada, tomaremos todos los datos
            string query = lastExportedDate.HasValue
                ? "SELECT datetime, value FROM EnergyDemand_Spain WHERE datetime > @LastExportedDate ORDER BY datetime"
                : "SELECT datetime, value FROM EnergyDemand_Spain ORDER BY datetime";

            List<CSVData> data = new List<CSVData>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                SqlCommand command = new SqlCommand(query, connection);

                if (lastExportedDate.HasValue)
                {
                    command.Parameters.AddWithValue("@LastExportedDate", lastExportedDate.Value);
                }

                SqlDataReader reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    DateTime datetime = reader.GetDateTime(0);
                    decimal value = reader.GetDecimal(1);

                    data.Add(new CSVData
                    {
                        DateTime = datetime,
                        Value = value
                    });
                }
            }

            return data;
        }

        public async Task<List<CSVDataRegion>> FetchRegionDataAsync()
        {
            DateTime? lastExportedDateRegion = _csvHandler.GetLastExportedDateRegion();
            string query = lastExportedDateRegion.HasValue
                ? "SELECT datetime, value, id_region FROM EnergyDemand_Region WHERE datetime > @LastExportedDateRegion ORDER BY datetime"
                : "SELECT datetime, value, id_region FROM EnergyDemand_Region ORDER BY datetime";

            List<CSVDataRegion> data = new List<CSVDataRegion>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                SqlCommand command = new SqlCommand(query, connection);

                if (lastExportedDateRegion.HasValue)
                {
                    command.Parameters.AddWithValue("@LastExportedDateRegion", lastExportedDateRegion.Value);
                }

                SqlDataReader reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    DateTime datetime = reader.GetDateTime(0);
                    decimal value = reader.GetDecimal(1);
                    int region = reader.GetInt32(2);

                    data.Add(new CSVDataRegion
                    {
                        DateTime = datetime,
                        Value = value,
                        Id_Region = region,
                    });
                }
            }

            return data;
        }

    }

}
