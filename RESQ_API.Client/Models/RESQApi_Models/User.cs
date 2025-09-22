using System;
using System.Collections.Generic;
using System.Text;

namespace RESQ_API.Client.Models.RESQApi_Models
{
    public class User
    {
        public int UserId { get; set; }   // Primary Key
        public string FullName { get; set; }
        public string UserName { get; set; } // Email
        public string PersonalPhoneNumber { get; set; }
        public string FamilyPhoneNumber { get; set; }
        public string Region { get; set; }
        public string District { get; set; }

        // Navigation property (1 User -> Many Events)
        public ICollection<EmergencyEvent> EmergencyEvents { get; set; }
    }
}
