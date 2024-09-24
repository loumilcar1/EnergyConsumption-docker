using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace Connector
{
    class CreateOrUpdateCSV
    {
        private readonly string _filePath;
        private readonly string _regionFilePath;

        public CreateOrUpdateCSV(IConfiguration configuration)
        {
            _filePath = configuration["AppSettings:CSVFilePath"];
            _regionFilePath = configuration["AppSettings:CSVRegionFilePath"];
        }

        public DateTime? GetLastExportedDate()
        {
            var csvFiles = Directory.GetFiles(_filePath, "SpainData_*.csv");

            if (csvFiles.Length == 0)
            {
                // Si no hay archivos, devolver null, lo que implica que no se ha exportado nada aún
                return null;
            }

            // Ordenar los archivos por nombre (año y mes) para obtener el más reciente
            var latestCsvFile = csvFiles.OrderByDescending(f => f).FirstOrDefault();

            // Leer el archivo y obtener la última línea (última fecha registrada)
            var lastLine = File.ReadLines(latestCsvFile).LastOrDefault();

            if (lastLine != null)
            {
                var lastRecord = lastLine.Split(',');
                if (DateTime.TryParse(lastRecord[0], out DateTime lastDateTime))
                {
                    return lastDateTime;
                }
            }

            return null; // Si algo falla, devuelve null
        }

        // Método para actualizar o crear CSVs según los datos recibidos
        public void UpdateCsv(List<CSVData> records)
        {
            // Agrupar los datos por mes
            var groupedByMonth = records
                .GroupBy(r => new { r.DateTime.Year, r.DateTime.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month);

            foreach (var group in groupedByMonth)
            {
                string monthYear = $"{group.Key.Year}_{group.Key.Month:D2}";
                string filePath = Path.Combine(_filePath, $"SpainData_{monthYear}.csv");

                if (!File.Exists(filePath))
                {
                    // Si el archivo no existe, crearlo
                    using (StreamWriter writer = new StreamWriter(filePath, false))
                    {
                        writer.WriteLine("DateTime,Value");
                        foreach (var record in group)
                        {
                            writer.WriteLine($"{record.DateTime},{record.Value}");
                            
                        }
                        Console.WriteLine($"Data successfully exported to CSV: {Path.GetFileName(filePath)}");
                    }
                }
                else
                {
                    // Si el archivo ya existe, añadir los nuevos registros
                    AppendToCsv(filePath, group.ToList());
                }
            }
        }
        private void AppendToCsv(string filePath, List<CSVData> records)
        {
            // Leer registros existentes del archivo
            var existingRecords = new HashSet<string>();

            if (File.Exists(filePath))
            {
                using (var reader = new StreamReader(filePath))
                {
                    reader.ReadLine(); // Omitir la línea de encabezado
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        existingRecords.Add(line); // Guardar cada registro existente en el conjunto (HashSet)
                    }
                }
            }

            // Añadir solo los registros que no existan ya
            using (var writer = new StreamWriter(filePath, true))
            {
                foreach (var record in records)
                {
                    string csvLine = $"{record.DateTime},{record.Value}";
                    if (!existingRecords.Contains(csvLine))
                    {
                        writer.WriteLine(csvLine); // Solo escribir si no es duplicado
                    }
                }
                Console.WriteLine($"Data successfully exported to CSV: {Path.GetFileName(filePath)}");
            }
            
        }

        //REGION
        public DateTime? GetLastExportedDateRegion()
        {
            // Buscar todos los archivos CSV que tengan el patrón "RegionData_*.csv" en la ruta especificada
            var csvFiles = Directory.GetFiles(_regionFilePath, "RegionData_*.csv");

            // Si no hay archivos, devolver null, lo que indica que aún no se ha exportado nada
            if (csvFiles.Length == 0) return null;

            // Ordenar los archivos por nombre (suponiendo que los nombres contienen año y mes)
            var latestCsvFile = csvFiles.OrderByDescending(f => f).FirstOrDefault();

            // Leer el archivo y obtener la última línea (es decir, la última fecha registrada)
            var lastLine = File.ReadLines(latestCsvFile).LastOrDefault();

            if (lastLine != null)
            {
                // Dividir la última línea por comas (asumiendo que el formato es "Region,DateTime,Value")
                var lastRecord = lastLine.Split(',');

                // Intentar analizar el segundo campo como una fecha
                if (DateTime.TryParse(lastRecord[0], out DateTime lastDateTime))
                {
                    return lastDateTime;
                }
            }

            // Si no se puede obtener la última fecha, devolver null
            return null;
        }
        // Nuevo método para actualizar o crear los CSV de EnergyConsumption_Region
        public void UpdateCsvRegion(List<CSVDataRegion> records)
        {
            var groupedByMonthRegion = records.GroupBy(r => new { r.DateTime.Year, r.DateTime.Month }).OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month);

            foreach (var group in groupedByMonthRegion)
            {
                string monthYear = $"{group.Key.Year}_{group.Key.Month:D2}";
                string filePath = Path.Combine(_regionFilePath, $"RegionData_{monthYear}.csv");

                if (!File.Exists(filePath))
                {
                    using (StreamWriter writer = new StreamWriter(filePath, false))
                    {
                        writer.WriteLine("DateTime,Region,Value");
                        foreach (var record in group)
                        {
                            writer.WriteLine($"{record.DateTime},{record.Id_Region},{record.Value}");
                        }
                    }
                }
                else
                {
                    AppendToCsvRegion(filePath, group.ToList());
                }
            }
        }

        // Método para añadir nuevos registros a los CSV de regiones
        private void AppendToCsvRegion(string filePath, List<CSVDataRegion> records)
        {
            // Leer registros existentes del archivo
            var existingRecords = new HashSet<string>();

            if (File.Exists(filePath))
            {
                using (var reader = new StreamReader(filePath))
                {
                    reader.ReadLine(); // Omitir la línea de encabezado
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        existingRecords.Add(line); // Guardar cada registro existente en el conjunto (HashSet)
                    }
                }
            }

            // Añadir solo los registros que no existan ya
            using (var writer = new StreamWriter(filePath, true))
            {
                foreach (var record in records)
                {
                    string csvLine = $"{record.DateTime},{record.Id_Region},{record.Value}";
                    if (!existingRecords.Contains(csvLine))
                    {
                        writer.WriteLine(csvLine); // Solo escribir si no es duplicado
                    }
                }
            }
            Console.WriteLine("\n");
            Console.WriteLine($"Data exported to CSV successfully: {Path.GetFileName(filePath)}");
        }
    }
}
