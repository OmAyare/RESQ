using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Servers.Areas.LiveTracking.Data
{
    public class EmergencyDetails
    {
        public int id { get; set; }
        public System.DateTime EventDateTime { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Status { get; set; }
        public System.Guid SessionId { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string PersonalPhone { get; set; }
        public string EmergencyPhone { get; set; }
        public string Region { get; set; }
        public string District { get; set; }
    }
}