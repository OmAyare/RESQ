using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RESQ_Server.Areas.Admin.Data
{
    [MetadataType(typeof(Emergency_EventResqMetaData))]
    public partial class Emergency_Events
    {
    }
    public class Emergency_EventResqMetaData
    {
        public int id { get; set; }
        public int UserId { get; set; }
        public System.DateTime EventDateTime { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Status { get; set; }

        public virtual user user { get; set; }
    }
}