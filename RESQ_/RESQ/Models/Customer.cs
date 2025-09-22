using SQLite;

namespace RESQ.Models
{
    public class Customer
    {
        [PrimaryKey, AutoIncrement]
        public int Cust_Id { get; set; }   // local PK

        public int? UserID { get; set; }   // from MSSQL, set at registration
        [Unique]
        public string Username { get; set; } // email
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Gender { get; set; }
        public DateTime? DOB { get; set; }
        public string Address { get; set; }
        public double? Height { get; set; }
        public double? Weight { get; set; }
        public string BloodType { get; set; }
        public string PH_BloodType { get; set; }
        public bool OrganDonor { get; set; }
        public string Remarks { get; set; }

        // MSSQL-only fields are skipped here (Personal_PhoneNumber, Emergency_PhoneNumber, Region, District).
        // They will be collected at registration, but only pushed to MSSQL, not stored locally.
    }
}
