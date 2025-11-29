using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using AndroidX.Core.App;
using CommunityToolkit.Mvvm.ComponentModel;
using RESQ.Models;
using RESQ.Services;
using RESQ.ViewModels;

namespace RESQ.Platforms.Android
{
    [Service(Exported = true, ForegroundServiceType = ForegroundService.TypeLocation)]
    public class EmergencyService : Service
    {
        private const int ServiceId = 1001;
        private Timer? _timer;
        private Customer customer;

        public override void OnCreate()
        {
            base.OnCreate();

            // Build persistent notification
            var notification = new NotificationCompat.Builder(this, "resq_emergency_channel")
                .SetContentTitle("RESQ Emergency Active")
                .SetContentText("Sharing live location every 30s")
                .SetSmallIcon(Resource.Mipmap.appicon)
                .SetOngoing(true)
                .Build();

            StartForeground(ServiceId, notification);
        }

        public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
        {
            // Resolve shared services via DI
            var locationService = ViewModels.ServiceHelper.GetService<ILocationService>();
            var eventService = ServiceHelper.GetService<IEmergencyEventService>();
            var localDb = ServiceHelper.GetService<RESQ.Data.LocalDatabase>();

            // Timer to run every 30 seconds
            _timer = new Timer(async _ =>
            {
                try
                {
                    if (customer == null)
                        customer = await localDb.GetCustomerAsync();

                    if (customer == null)
                    {
                        Console.WriteLine("❌ No customer found in local DB.");
                        return;
                    }

                    var (lat, lng) = await locationService.GetCurrentLocationAsync();

                    var ev = new RESQ.Models.EmergencyEvent
                    {
                        Cust_Id = customer.Cust_Id, // TODO: map from logged in Customer
                        EventDateTime = DateTime.UtcNow,
                        Latitude = lat,
                        Longitude = lng,
                        Status = "EMERGENCY"
                    };

                    // await eventService.SaveAndSendEventAsync(ev);
                    await eventService.SendUpdateAsync(lat, lng);

                }
                catch (Exception ex)
                {
                    //Android.Util.Log.Error("RESQ", $"❌ Emergency loop error: {ex.Message}");
                    Console.WriteLine($"Emergency loop error: { ex.Message}");
                }

            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));

            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            _timer?.Dispose();
            _timer = null;
            base.OnDestroy();
        }

        public override IBinder? OnBind(Intent? intent) => null;
    }
}
