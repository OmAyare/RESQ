using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RESQ.Models;

namespace RESQ.Services
{
    //public interface IEmergencyEventService
    //{
    //    Task SaveAndSendEventAsync(EmergencyEvent ev);
    //}
    public interface IEmergencyEventService
    {
        Task TriggerEmergencyAsync();
        Task SendUpdateAsync(double lat, double lng);
        Task EndEmergency();
    }
}
