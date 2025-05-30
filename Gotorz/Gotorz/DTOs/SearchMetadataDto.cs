// Gotorz.DTOs/SearchMetadataDto.cs
using System.Text.Json.Serialization;

namespace Gotorz.DTOs
{
    public class SearchMetadataDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("json_endpoint")]
        public string JsonEndpoint { get; set; }

        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; }

        [JsonPropertyName("processed_at")]
        public string ProcessedAt { get; set; }

        [JsonPropertyName("google_flights_url")]
        public string GoogleFlightsUrl { get; set; }

        [JsonPropertyName("raw_html_file")]
        public string RawHtmlFile { get; set; }

        [JsonPropertyName("prettify_html_file")]
        public string PrettifyHtmlFile { get; set; }

        [JsonPropertyName("total_time_taken")]
        public float TotalTimeTaken { get; set; }
    }
}
