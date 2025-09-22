using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace RESQ_API.Domain.Entities
{
    [Table("users")]
    [Index(nameof(UserName), IsUnique = true)]
    public record User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public int UserId { get; set; }   

        [Required]
        [StringLength(50)]
        [Column("full_Name")]
        public string FullName { get; set; }

        [Required]
        [StringLength(50)]
        [DataType(DataType.EmailAddress)]
        public string UserName { get; set; }

        [Required,StringLength(30)]
        [DataType(DataType.PhoneNumber)]
        [Column("Personal_PhoneNumber")]
        public string PersonalPhoneNumber { get; set; }

        [Required, StringLength(30)]
        [DataType(DataType.PhoneNumber)]
        [Column("Emergency_PhoneNumber")]
        public string FamilyPhoneNumber { get; set; }

        [Required, StringLength(50)]
        public string Region { get; set; }

        [Required, StringLength(50)]
        public string District { get; set; }

        // Navigation property (1 User -> Many Events)
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public ICollection<EmergencyEvent> EmergencyEvents { get; set; } = new List<EmergencyEvent>();
    }
}
