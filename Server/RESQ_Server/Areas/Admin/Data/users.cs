using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RESQ_Server.Areas.Admin.Data
{
    [MetadataType(typeof(UserResqMetaData))]
    public partial class user
    {
    }
    public class UserResqMetaData
    {
        [Key]
        public int id { get; set; }

        public string full_Name { get; set; }
        public string UserName { get; set; }
        public string Personal_PhoneNumber { get; set; }
        public string Emergency_PhoneNumber { get; set; }
        public string Region { get; set; }
        public string District { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Emergency_Events> Emergency_Events { get; set; }

    }
}