using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Servers.Areas.LiveTracking.Data
{
    public class Emergencyrecords
    {
        public int EventId { get; set; }

        [Display(Name = "Name")]
        public string FullName { get; set; }

        [Display(Name = "State")]
        public string Region { get; set; }

        public string District { get; set; }

        [Display(Name = "Date & Time")]
        public DateTime EventDateTime { get; set; }
    }
}