using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RESQ_Server.Areas.Admin.Data
{
    [MetadataType(typeof(UserMetaData))]
    public partial class User
    {
    }

    public class UserMetaData
    {
        [Key]
        public int User_id { get; set; }


        [Required(ErrorMessage = "The Name is reuired")]
        [Remote("IsUsernameAvailable", "Admin", ErrorMessage = "Username already Exist")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Username must be between 3 and 50 characters.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Email is Required")]
        [Remote("IsEmailAvailable", "Admin", ErrorMessage = "Email already Exist")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [StringLength(100, MinimumLength = 2)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Need to Assign the Role to the User")]
        [Display(Name ="Role")]
        public Nullable<int> Role_id { get; set; }
        public string FullName { get; set; }
        public string FatherName { get; set; }
        public string MotherName { get; set; }
        public string Gender { get; set; }

        [DataType(DataType.Date)]
        public Nullable<System.DateTime> DateOfBirth { get; set; }
        [DataType(DataType.Date)]
        public Nullable<System.DateTime> JoiningDate { get; set; }
        public string Branch { get; set; }
        public string AadharNumber { get; set; }
        public string UANNumber { get; set; }
        public string BloodGroup { get; set; }
        public string BankAccount_1 { get; set; }
        public string BankAccount_2 { get; set; }
        public string PhoneNumber { get; set; }
        public string Employee_Photo { get; set; }

        [ForeignKey("Role_id")]
        public virtual Role Role { get; set; }
    }

}