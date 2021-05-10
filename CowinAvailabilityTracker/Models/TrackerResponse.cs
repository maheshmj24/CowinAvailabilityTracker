using System.Collections.Generic;

namespace CowinAvailabilityTracker
{
    public class TrackerResponse
    {
        public int AvailableCount { get; set; }
        public int AvailableCountCountLt45 { get; set; }
        public List<AvailableSession> AvailableSessions { get; set; }
    }

    public class AvailableSession
    {
        public string Name { get; set; }
        public double Available_capacity { get; set; }
        public int Min_age_limit { get; set; }
    }
}
