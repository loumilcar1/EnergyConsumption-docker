using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Newtonsoft.Json;
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
            startDate = configuration["AppSettings:StartDate"];
            endDate = configuration["AppSettings:EndDate"];
            timeTrunc = configuration["AppSettings:TimeTrunc"];
            geoLimit = configuration["AppSettings:GeoLimit"];
            geoIds = configuration["AppSettings:GeoIds"];
            useConfigurableUrl = bool.Parse(configuration["AppSettings:UseConfigurableUrl"]);

            _connectionString = configuration.GetConnectionString("EnergyConsumptionDB");
        }

        public async Task<(string, Dictionary<int, string>)> FetchDataAsync()
        {
            string urlSpain = null;
            Dictionary<int, string> responseRegions = new Dictionary<int, string>();

            if (useConfigurableUrl)
            {
                // Usar la URL configurable para realizar la solicitud
                var configurableUrlSpain = $"{baseUrl}?start_date={startDate}&end_date={endDate}&time_trunc={timeTrunc}&geo_limit={geoLimit}&geo_ids={geoIds}";

                try
                {

                    // Enviar solicitud GET a la API
                    HttpResponseMessage response = await client.GetAsync(configurableUrlSpain);
                    response.EnsureSuccessStatusCode();

                    // Obtener la respuesta como string
                    string responseSpain = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("1- Data fetched from urlConfigurable:\n");
                    Console.WriteLine(responseSpain);

                    var data = JsonConvert.DeserializeObject<JsonData>(responseSpain);

                    if (data?.Included == null)
                    {
                        throw new ArgumentNullException(nameof(data.Included), "The JSON does not contain any data");
                    }

                    var values = data.Included[0].Attributes.Values;
                    var parsedData = new (DateTime datetime, decimal value)[values.Count];

                    int index = 0;

                    foreach (var item in values)
                    {
                        DateTime datetime = DateTime.Parse(item.Datetime);
                        decimal value = item.value;
                        parsedData[index++] = (datetime, value);
                    }

                    // Print parsed data
                    Console.WriteLine("\n");
                    Console.WriteLine("2- Data parsed:\n");
                    Console.WriteLine("Date \t\t\t\t Value");
                    foreach (var (datetime, value) in parsedData)
                    {
                        Console.WriteLine($"{datetime}\t\t{value}");
                    }
                }

                catch (HttpRequestException e)
                {
                    Console.WriteLine("\n¡Excepción atrapada!");
                    Console.WriteLine("Mensaje: {0} ", e.Message);
                }

                // Retornar una tupla vacía o valores por defecto para cumplir con la firma de retorno
                return (null, null);
            }
            else
            {
                // Determine the start date based on the last date in the database
                DatabaseHandler dbHandler = new DatabaseHandler(_connectionString);
                DateTime lastDateSpainInDb = await dbHandler.GetLastDateSpainAsync();
                DateTime today = DateTime.Today;

                // Construct urlSpain
                if (lastDateSpainInDb.Date < today)
                {
                    DateTime startDateSpain = lastDateSpainInDb.AddDays(1);
                    urlSpain = $"{baseUrl}?start_date={startDateSpain:yyyy-MM-dd}T00:00&end_date={today:yyyy-MM-dd}T23:59&time_trunc=day";
                    //urlSpain = "https://apidatos.ree.es/es/datos/demanda/evolucion?start_date=2011-01-01T00:00&end_date=2011-12-31T23:59&time_trunc=day";
                }

                // Prepare to collect responses for all regions
                List<Task<(int, string)>> regionTasks = new List<Task<(int, string)>>();
                
                Console.WriteLine($"1- Data fetched from urlRegion: \n ");
                // Iterate through each region and create its URL
                foreach (var region in RegionConfigurations.Configurations.OrderBy(r => r.Key))
                {
                    DateTime lastDateRegionInDb = await dbHandler.GetLastDateRegionAsync(region.Key);

                    if (lastDateRegionInDb.Month != today.Month || lastDateRegionInDb.Year != today.Year)
                    {
                        DateTime startDateRegion = new DateTime(lastDateRegionInDb.Year, lastDateRegionInDb.Month, 1).AddMonths(1);
                        string regionUrl = $"{baseUrl}?start_date={startDateRegion:yyyy-MM-dd}T00:00&end_date={today:yyyy-MM-dd}T23:59&time_trunc=month&geo_limit={region.Value.geoLimit}&geo_ids={region.Value.geoId}";
                        //string regionUrl = $"https://apidatos.ree.es/es/datos/demanda/evolucion?start_date=2011-01-01T00:00&end_date=2011-12-31T23:59&time_trunc=month&geo_limit={region.Value.geoLimit}&geo_ids={region.Value.geoId}";

                        //Console.WriteLine($"Fetching data for region {region.Key} with URL: {regionUrl}");

                        // Add the task to fetch region data
                        regionTasks.Add(FetchRegionDataAsync(region.Key, regionUrl));
                    }
                }

                // Await all region data fetches, handling exceptions for each task
                var regionResults = await Task.WhenAll(regionTasks);

                // Populate the responseRegions dictionary
                foreach (var (regionId, regionJson) in regionResults)
                {
                    if (!string.IsNullOrEmpty(regionJson))
                    {
                        responseRegions[regionId] = regionJson;
                    }
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
                    Console.WriteLine("\n1- Data fetched from urlSpain:\n");
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
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
                throw;
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
                Console.WriteLine($"- Data for Region {regionId}: Successful \n {regionData} ");
                Console.WriteLine("\n");

                return (regionId, regionData);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"- Data for Region {regionId}: Failed\n ");
                //Console.WriteLine($"\nHttpRequestException Caught for region {regionId}!");
                //Console.WriteLine("Message :{0} ", e.Message);
                return (regionId, null);
            }
            catch (Exception e)
            {
                //Console.WriteLine($"\nGeneral Exception Caught for region {regionId}!");
                //Console.WriteLine("Message :{0} ", e.Message);
                return (regionId, null);
            }
        }
    }
}