using System;
using System.Collections.Generic;
using System.Text;

namespace RESQ_API.Client.Models.RESQApi_Models
{
    public class EmergencyEvent
    {
        public int EventId { get; set; }  // Primary Key
        public int UserId { get; set; }   // Foreign Key

        public DateTime EventDateTime { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Status { get; set; }

        // Navigation property (Event belongs to 1 User)
        public User User { get; set; }
    }
}
