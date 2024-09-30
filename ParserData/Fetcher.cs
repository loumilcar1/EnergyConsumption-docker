using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace ParserData
{
    class Fetcher
    {
        private static readonly HttpClient client = new HttpClient();

        // Configurable URL parameters from App.config
        private readonly string baseUrl;
        private readonly string startDate;
        private readonly string endDate;
        private readonly string timeTrunc;
        private readonly string geoLimit;
        private readonly string geoIds;
        private readonly bool useConfigurableUrl;

        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        public Fetcher(IConfiguration configuration)
        {
            _configuration = configuration;
            baseUrl = configuration["AppSettings:ApiBaseUrl"];
            _connectionString = configuration.GetConnectionString("EnergyConsumptionDB");
        }

        public async Task<(string, Dictionary<int, string>)> FetchDataAsync()
        {
            string urlSpain = null;

            // Obtener fecha de los últimos datos registrados
            DatabaseHandler dbHandler = new DatabaseHandler(_connectionString);
            DateTime lastDateSpainInDb = await dbHandler.GetLastDateSpainAsync();

            DateTime today = DateTime.Today;

            // Construir la url de consulta para obtener datos de España
            if (lastDateSpainInDb.Date < today)
            {
                DateTime startDateSpain = lastDateSpainInDb.AddDays(1);
                urlSpain = $"{baseUrl}?start_date={startDateSpain:yyyy-MM-dd}T00:00&end_date={today:yyyy-MM-dd}T23:59&time_trunc=day";
            }

            // Construir las urls de las consultas para obtener datos de cada región
            List<Task<(int, string)>> regionTasks = new List<Task<(int, string)>>();

            Console.WriteLine($"1- Region data fetched from REE: \n ");
            // Iterate through each region and create its URL
            foreach (var region in RegionConfigurations.Configurations.OrderBy(r => r.Key))
            {
                DateTime lastDateRegionInDb = await dbHandler.GetLastDateRegionAsync(region.Key);

                if (lastDateRegionInDb.Month != today.Month || lastDateRegionInDb.Year != today.Year)
                {
                    DateTime startDateRegion = new DateTime(lastDateRegionInDb.Year, lastDateRegionInDb.Month, 1).AddMonths(1);
                    string regionUrl = $"{baseUrl}?start_date={startDateRegion:yyyy-MM-dd}T00:00&end_date={today:yyyy-MM-dd}T23:59&time_trunc=month&geo_limit={region.Value.geoLimit}&geo_ids={region.Value.geoId}";

                    // Add the task to fetch region data
                    regionTasks.Add(FetchRegionDataAsync(region.Key, regionUrl));
                }
            }

            // Await all region data fetches, handling exceptions for each task
            var regionResults = await Task.WhenAll(regionTasks);

            Dictionary<int, string> responseRegions = new Dictionary<int, string>();

            // Populate the responseRegions dictionary
            foreach (var (regionId, regionJson) in regionResults)
            {
                if (!string.IsNullOrEmpty(regionJson))
                {
                    responseRegions[regionId] = regionJson;
                }
            }

            if (urlSpain == null && responseRegions.Count == 0)
            {
                return (null, null);
            }

            try
            {
                string responseSpain = null;
                if (urlSpain != null)
                {
                    // Send GET request to the API for urlSpain
                    HttpResponseMessage response1 = await client.GetAsync(urlSpain);
                    response1.EnsureSuccessStatusCode();
                    responseSpain = await response1.Content.ReadAsStringAsync();

                    // Print the fetched data from urlSpain
                    Console.WriteLine("\n1- Spain data fetched from REE:\n");
                    Console.WriteLine(responseSpain);
                    Console.WriteLine("\n");
                    Console.WriteLine("\n");
                }
                // Print the fetched data from urlRegion
                //Console.WriteLine("Data fetched from urlRegion:");

                if (responseSpain == null)
                {
                    Console.WriteLine("No data fetched from Spain. Data is already up to date.");
                }
                if (responseRegions == null)
                {
                    Console.WriteLine("No data fetched from regions. Data is already up to date.");
                }
                return (responseSpain, responseRegions);
            }
            catch (HttpRequestException e)
            {
                throw new Exception("\"Error: {0}", e);
            }
        }

        private async Task<(int, string)> FetchRegionDataAsync(int regionId, string regionUrl)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(regionUrl);
                response.EnsureSuccessStatusCode();
                string regionData = await response.Content.ReadAsStringAsync();

                // Check if the response contains an error
                if (regionData.Contains("\"errors\""))
                {
                    Console.WriteLine($"No data for region {regionId}: {regionData}");
                    return (regionId, null); // Indicate no data by returning null
                }
                Console.WriteLine($"    - Region {regionId} data read from REE successfully:\n{regionData}");
                Console.WriteLine("\n");

                return (regionId, regionData);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"    - Region {regionId} data has not been provided by REE.\n ");
                return (regionId, null);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error:{0} ", e.Message);
                return (regionId, null);
            }
        }
    }
}