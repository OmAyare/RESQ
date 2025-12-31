using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Servers.Areas.LiveTracking.Data
{
    public class Emergencyrecords
    {
        public int EventId { get; set; }
        public string FullName { get; set; }
        public string Region { get; set; }
        public string District { get; set; }
        public DateTime EventDateTime { get; set; }
    }
}