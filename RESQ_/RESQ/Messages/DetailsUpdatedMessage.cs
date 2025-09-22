using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace RESQ.Messages
{
    public class DetailsUpdatedMessage : ValueChangedMessage<bool>
    {
        public DetailsUpdatedMessage() : base(true) { }
    }
}