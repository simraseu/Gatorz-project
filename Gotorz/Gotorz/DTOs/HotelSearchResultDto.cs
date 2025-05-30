using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Gotorz.DTOs
{
    public class HotelSearchResultDto
    {
        [JsonPropertyName("properties")]
        public List<HotelPropertyDto> Properties { get; set; }
    }
}
