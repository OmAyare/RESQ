using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RESQ.Services;

namespace RESQ.Platforms.Android
{
    public static class EmergencyBackgroundTrigger
    {
        public static void Trigger()
        {
            // 1. Persist emergency state
            Preferences.Set("EmergencyStatus", "EMERGENCY");
            Preferences.Set("EmergencyActive", true);

            // 2. Start emergency logic (SMS, location, DB)
            Task.Run(async () =>
            {
                var svc = MauiApplication.Current.Services
                    .GetService<IEmergencyEventService>();

                if (svc != null)
                    await svc.TriggerEmergencyAsync();
            });
        }
    }
}
