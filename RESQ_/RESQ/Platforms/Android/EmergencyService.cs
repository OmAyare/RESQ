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
using RESQ.Data;
using RESQ.Models;
using RESQ.Services;
using RESQ.ViewModels;
using RESQ_API.Client;
using Xamarin.Google.Crypto.Tink.Shaded.Protobuf;

namespace RESQ.Platforms.Android
{
    [Service(Exported = true, ForegroundServiceType = ForegroundService.TypeLocation)]
    public class EmergencyService : Service
    {
        private const int ServiceId = 1001;
        private Timer? _timer;
        private Customer customer;
        private LocalDatabase _db;
        private ApiSessionService _sessionService;
        private RESQApiClientService _api;
        private bool _isSyncing;

        public override void OnCreate()
        {
            base.OnCreate();

            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;

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
                _db ??= ServiceHelper.GetService<LocalDatabase>();
                _sessionService ??= ServiceHelper.GetService<ApiSessionService>();
                _api ??= ServiceHelper.GetService<RESQApiClientService>();

                try
                {
                    var status = Preferences.Get("EmergencyStatus", "SAFE");
                    if (status != "EMERGENCY")
                    {
                        StopSelf();
                        return;
                    }

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
                        Cust_Id = customer.Cust_Id, //  map from logged in Customer
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
                    Console.WriteLine($"Emergency loop error: {ex.Message}");
                }

            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));

            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;

            _timer?.Dispose();
            _timer = null;
            base.OnDestroy();
        }

        public override IBinder? OnBind(Intent? intent) => null;

        private async void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            if (e.NetworkAccess == NetworkAccess.Internet)
            {
                await SyncPendingEventsAsync();
            }
        }
        private RESQ_API.Client.Models.RESQApi_Models.EmergencyEvent MapToApi(EmergencyEvent local, int userId)
        {
            return new RESQ_API.Client.Models.RESQApi_Models.EmergencyEvent
            {
                //EventId = local.EventID,
                UserId = userId,
                Latitude = local.Latitude ?? 0,
                Longitude = local.Longitude ?? 0,
                Status = local.Status,
                SessionId = local.SessionId,
                EventDateTime = local.EventDateTime
            };
        }

        private bool HasInternet()
        {
            return Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
        }
        //public async Task SyncPendingEventsAsync()
        //{
        //    if (_isSyncing) return;
        //    _isSyncing = true;

        //    try
        //    {
        //        if (!HasInternet()) return;

        //        var customer = await _db.GetCustomerAsync();
        //        if (customer?.UserID == null) return;

        //        var unsynced = (await _db.GetUnsyncedEventsAsync())
        //            .OrderBy(e => e.EventDateTime)
        //            .ToList();

        //        if (!unsynced.Any()) return;

        //        // 1️⃣ Try session from memory first
        //        var sessionId = _sessionService.GetSession();

        //        // 2️⃣ If not in memory, try DB (SAFE events may already have it)
        //        if (sessionId == null)
        //        {
        //            sessionId = unsynced
        //                .Where(e => e.SessionId != Guid.Empty)
        //                .Select(e => (Guid?)e.SessionId)
        //                .FirstOrDefault();
        //        }

        //        // 3️⃣ Still no session → create ONE from earliest EMERGENCY
        //        if (sessionId == null)
        //        {
        //            var emergencySeed = unsynced
        //                .FirstOrDefault(e => e.Status == "EMERGENCY");

        //            if (emergencySeed == null)
        //            {
        //                // No emergency context → nothing to sync
        //                return;
        //            }

        //            var created = await _api.CreateEmergencyEventAsync(
        //                MapToApi(emergencySeed, customer.UserID.Value));

        //            sessionId = created.SessionId;
        //            _sessionService.SaveSession(sessionId.Value);
        //        }

        //        // 4️⃣ Replay all unsynced events
        //        foreach (var ev in unsynced)
        //        {
        //            ev.SessionId = sessionId.Value;

        //            await _api.UpdateSessionAsync(
        //                sessionId.Value,
        //                MapToApi(ev, customer.UserID.Value));

        //            ev.IsSynced = true;
        //            await _db.UpdateEmergencyEventAsync(ev);
        //        }
        //    }
        //    finally
        //    {
        //        _isSyncing = false;
        //    }
        //}

        public async Task SyncPendingEventsAsync()
        {
            if (_isSyncing) return;
            _isSyncing = true;

            try
            {
                if (!HasInternet()) return;

                var customer = await _db.GetCustomerAsync();
                if (customer?.UserID == null) return;

                //ONLY SAFE + UNSYNCED
                var safeUnsynced = (await _db.GetUnsyncedEventsAsync())
                    .Where(e => e.Status == "Safe" || e.Status == "ACTIVE")
                    .OrderBy(e => e.EventDateTime)
                    .ToList();

                if (!safeUnsynced.Any()) return;

                // Check existing session
                var sessionId = _sessionService.GetSession();

                //If no session, create ONE from the first SAFE event
                if (sessionId == null)
                {
                    var firstSafe = safeUnsynced.First();

                    var created = await _api.CreateEmergencyEventAsync(
                        MapToApi(firstSafe, customer.UserID.Value));

                    sessionId = created.SessionId;
                    _sessionService.SaveSession(sessionId.Value);
                }

                //Sync all SAFE events using the session
                foreach (var ev in safeUnsynced)
                {
                    ev.SessionId = sessionId.Value;

                    await _api.UpdateSessionAsync(
                        sessionId.Value,
                        MapToApi(ev, customer.UserID.Value));

                    ev.IsSynced = true;
                    await _db.UpdateEmergencyEventAsync(ev);
                }
            }
            finally
            {
                _isSyncing = false;
            }
        }

    }
}
