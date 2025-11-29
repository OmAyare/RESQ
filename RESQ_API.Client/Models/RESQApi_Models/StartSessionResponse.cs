using System;
using System.Collections.Generic;
using System.Text;

namespace RESQ_API.Client.Models.RESQApi_Models
{
    public class StartSessionResponse
    {
        public Guid SessionId { get; set; }
        public int EventId { get; set; }
    }

}
