using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RESQ_API.Domain.Entities
{
    [Table("Emergency_Events")]
    public record EmergencyEvent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int EventId { get; set; }  // Primary Key
        public int UserId { get; set; }   // Foreign Key

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime EventDateTime { get; set; } 

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }
        public string? Status { get; set; }
        public Guid? SessionId { get; set; }

        // Navigation property (Event belongs to 1 User)
        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
    }
}
