using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RESQ_Server.Common;
using RESQ_Server.Models;

namespace RESQ_Server.Areas.Admin.Data
{
    [MetadataType(typeof(RoleMetaData))]
    public partial class Role
    {

    }

    public class RoleMetaData
    {
        [Key]
        public int Id { get; set; }

        [Remote("IsRoleAvailable", "Admin",ErrorMessage ="Role already Exist")]
       // [RemoteClientServer("IsRoleAvailable", "Admin", ErrorMessage = "Role already Exist")]
        [Required(ErrorMessage = "The Role Field should not be empty")]
        [StringLength(30)]
        public string Name { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<User> Users { get; set; }
    }
}