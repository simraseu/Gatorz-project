using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Gotorz.DTOs
{
    public class HotelPropertyDto
    {
        [JsonPropertyName("property_id")]
        public string PropertyId { get; set; }

        [JsonPropertyName("property_token")]
        public string PropertyToken { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }
    }
}
