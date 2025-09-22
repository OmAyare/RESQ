using SQLite;


namespace RESQ.Models
{
    public class EmergencyContact
    {
        [PrimaryKey, AutoIncrement]
        public int ContactID { get; set; }

        public int Cust_Id { get; set; } // FK to Customer
        public string ContactName { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsFixed112 => PhoneNumber == "112";
    }
}
