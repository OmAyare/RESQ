using SQLite;

namespace RESQ.Models
{
    public class EmergencyEvent
    {
        [PrimaryKey, AutoIncrement]
        public int EventID { get; set; }
        public int Cust_Id { get; set; } // FK to Customer
        public DateTime EventDateTime { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Status { get; set; }
        public Guid SessionId { get; set; }
        public bool IsSynced { get; set; }   
    }
}
