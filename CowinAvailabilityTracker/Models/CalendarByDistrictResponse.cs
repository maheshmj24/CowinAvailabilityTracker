using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CowinAvailabilityTracker
{
    public class CalendarByDistrictResponse
    {
        [JsonPropertyName("centers")]
        public List<Center> Centers { get; set; }
    }

    public class Center
    {
        [JsonPropertyName("center_id")]
        public int Center_id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("sessions")]
        public List<Session> Sessions { get; set; }
    }

    public class Session
    {
        [JsonPropertyName("available_capacity")]
        public double Available_capacity { get; set; }

        [JsonPropertyName("min_age_limit")]
        public int Min_age_limit { get; set; }
    }
}
