using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ParserData
{
    public class Parser
    {
        public static ((DateTime datetime, decimal value)[], (DateTime datetime, decimal value, int id_region)[]) ParserData(string jsonSpain, Dictionary<int, string> jsonRegion)
        {
            // Parse data for Spain
            var dataSpain = string.IsNullOrEmpty(jsonSpain) ? null : ParseJsonSpain(jsonSpain);

            // Parse data for Region if jsonRegion is not null and has data
            var dataRegion = jsonRegion != null && jsonRegion.Count > 0 ? ParseJsonRegion(jsonRegion) : null;

            return (dataSpain, dataRegion);
        }
        private static (DateTime datetime, decimal value)[] ParseJsonSpain(string jsonSpain)
        {
            var data = JsonConvert.DeserializeObject<JsonData>(jsonSpain);

            if (data?.Included == null)
            {
                throw new ArgumentNullException(nameof(data.Included), "Error: The JSON does not contain any data");
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
            Console.WriteLine("2- Spain data parsed:\n");
            Console.WriteLine("Date \t\t\t\t Value");
            foreach (var (datetime, value) in parsedData)
            {
                Console.WriteLine($"{datetime}\t\t{value}");
            }

            return parsedData;
        }

        private static (DateTime datetime, decimal value, int id_region)[] ParseJsonRegion(Dictionary<int, string> jsonRegionMap)
        {
            var allParsedData = new List<(DateTime datetime, decimal value, int id_region)>();
        

            foreach (var kvp in jsonRegionMap)
            {
                int regionId = kvp.Key;
                string regionJson = kvp.Value;

                var data = JsonConvert.DeserializeObject<JsonData>(regionJson);

                if (data?.Included == null)
                {
                    throw new ArgumentNullException(nameof(data.Included), "Error: The JSON does not contain any data");
                }

                var values = data.Included[0].Attributes.Values;

                foreach (var item in values)
                {
                    DateTime datetime = DateTime.Parse(item.Datetime);
                    decimal value = item.value;
                    allParsedData.Add((datetime, value, regionId));
                }
            }
            // Print parsed data
            Console.WriteLine("\n");
            Console.WriteLine("2- Region data parsed:\n");;
            Console.WriteLine("Date \t\t\t Value\t\t IdRegion");
            foreach (var (datetime, value, id_region) in allParsedData)
            {
                Console.WriteLine($"{datetime:yyyy-MM-dd HH:mm:ss}\t{value}\t\t{id_region}");
            }

            return allParsedData.ToArray();
        }
     
    }
}