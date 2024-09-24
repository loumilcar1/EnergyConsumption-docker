using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ParserData
{
    public class JsonData
    {
        [JsonPropertyName("data")]
        public Data Data { get; set; }

        [JsonPropertyName("included")]
        public List<Included> Included { get; set; }
    }

    public class Data
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("attributes")]
        public Attributes Attributes { get; set; }

        [JsonPropertyName("meta")]
        public Meta Meta { get; set; }
    }

    public class Attributes
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("last-update")]
        public string LastUpdate { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }
    }

    public class Meta
    {
        [JsonPropertyName("cache-control")]
        public CacheControl CacheControl { get; set; }
    }

    public class CacheControl
    {
        [JsonPropertyName("cache")]
        public string Cache { get; set; }
    }

    public class Included
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("groupId")]
        public string GroupId { get; set; }

        [JsonPropertyName("attributes")]
        public IncludedAttributes Attributes { get; set; }
    }

    public class IncludedAttributes
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("color")]
        public string Color { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("magnitude")]
        public string Magnitude { get; set; }

        [JsonPropertyName("last-update")]
        public string LastUpdate { get; set; }

        [JsonPropertyName("values")]
        public List<Value> Values { get; set; }
    }

    public class Value
    {
        [JsonPropertyName("value")]
        public decimal value { get; set; }

        [JsonPropertyName("datetime")]
        public string Datetime { get; set; }
    }

}


