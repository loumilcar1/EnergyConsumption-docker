using System.Collections.Generic;

namespace ParserData
{
    class RegionConfigurations
    {
        public static readonly Dictionary<int, (string geoLimit, string geoId)> Configurations = new Dictionary<int, (string geoLimit, string geoId)>
        {
            { 1, ("peninsular", "8741") },
            { 2, ("canarias", "8742") },
            { 3, ("baleares", "8743") },
            { 4, ("ceuta", "8744") },
            { 5, ("melilla", "8745") },
            { 6, ("ccaa", "4") },
            { 7, ("ccaa", "5") },
            { 8, ("ccaa", "6") },
            { 9, ("ccaa", "7") },
            { 10, ("ccaa", "8") },
            { 11, ("ccaa", "9") },
            { 12, ("ccaa", "10") },
            { 13, ("ccaa", "11") },
            { 14, ("ccaa", "8744") }, // Comunidad de Ceuta (mismo geo_id que Ceuta)
            { 15, ("ccaa", "8745") }, // Comunidad de Melilla (mismo geo_id que Melilla)
            { 16, ("ccaa", "13") },
            { 17, ("ccaa", "14") },
            { 18, ("ccaa", "15") },
            { 19, ("ccaa", "16") },
            { 20, ("ccaa", "17") },
            { 21, ("ccaa", "8743") }, // Islas Baleares (mismo geo_id que Baleares)
            { 22, ("ccaa", "8742") }, // Islas Canarias (mismo geo_id que Canarias)
            { 23, ("ccaa", "20") },
            { 24, ("ccaa", "21") }
        };
    }
}
