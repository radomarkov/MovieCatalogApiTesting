using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace MovieCatalogApiTesting.Models
{
    public class MovieDTO
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("msg")]
        public string Msg { get; set; }
    }
}
