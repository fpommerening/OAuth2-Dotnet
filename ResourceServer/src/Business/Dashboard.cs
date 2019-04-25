using System;
using System.Collections.Generic;

namespace FP.OAuth.ResourceServer.Business
{
    public class Dashboard
    {
        public Dashboard()
        {
            Timestamp = DateTime.UtcNow;
        }

        public DateTime Timestamp { get; set; }

        public List<ValueItem> ValueItems { get; set; }
    }
}
