using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Gotorz.DTOs
{
    public class RateDto
    {
        [JsonPropertyName("lowest")]
        public Decimal Lowest { get; set; }

        [JsonPropertyName("extracted_lowest")]
        public decimal ExtractedLowest { get; set; }
    }
}
