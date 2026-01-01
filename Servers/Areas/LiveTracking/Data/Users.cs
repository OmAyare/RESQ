using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Servers.Areas.LiveTracking.Data
{
    [MetadataType(typeof(RESQUserMetaData))]
    public partial class user
    {
    }

    public class RESQUserMetaData
    {
        public int id { get; set; }

        [Display(Name = "Name")]
        public string full_Name { get; set; }

        [Display(Name = "Email")]
        public string UserName { get; set; }

        [Display(Name = "Personal Number")]
        public string Personal_PhoneNumber { get; set; }

        [Display(Name = "Family Number")]
        public string Emergency_PhoneNumber { get; set; }

        [Display(Name = "State")]
        public string Region { get; set; }
        public string District { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Emergency_Events> Emergency_Events { get; set; }
    }
}