using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Servers.Models
{
    [MetadataType(typeof(UserMetaData))]
    public partial class Resqemployee
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

        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Display(Name = "Father Name")]
        public string FatherName { get; set; }


        [Display(Name = "Mother Name")]
        public string MotherName { get; set; }
        public string Gender { get; set; }

        [Display(Name = "Date Of Birth")]
        [DataType(DataType.Date)]
        public Nullable<System.DateTime> DateOfBirth { get; set; }

        [Display(Name = "Joining Date")]
        [DataType(DataType.Date)]
        public Nullable<System.DateTime> JoiningDate { get; set; }
        public string Branch { get; set; }

        [Display(Name ="AadharCard Number")]
        public string AadharNumber { get; set; }
        public string UANNumber { get; set; }

        [Display(Name = "Blood Group")]
        public string BloodGroup { get; set; }

        [Display(Name = "Bank Account")]
        public string BankAccount_1 { get; set; }

        [Display(Name = "Bank Account 2")]
        public string BankAccount_2 { get; set; }

        [Display(Name = "Contact Number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Employee Photo")]
        public string Employee_Photo { get; set; }

        [ForeignKey("Role_id")]
        public virtual Role Role { get; set; }
    }

}