using SQLite;

namespace RESQ.Models
{
    public class MedicalInfo
    {
        [PrimaryKey, AutoIncrement]
        public int MedID { get; set; }

        public int Cust_Id { get; set; } // FK to Customer
        public string MedicalConditions { get; set; }
        public string Medications { get; set; }
        public string AllergiesReactions { get; set; }
    }
}
