using RESQ.Data;
using RESQ.Models;
using RESQ.Services;
using RESQ.Views;
using RESQ_API.Client;
using Xamarin.Google.Crypto.Tink.Shaded.Protobuf;

namespace RESQ
{
    public partial class App : Application
    {
        private readonly LocalDatabase _db;
        private readonly ApiSessionService _sessionService;
        private RESQApiClientService _api;
        private bool _isSyncing;
        public App(LoginPage loginPage, LocalDatabase db, ApiSessionService session, RESQApiClientService api)
        {
            _db = db;
            _sessionService = session;
            _api = api;


            Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;

            InitializeComponent();

            MainPage = new NavigationPage(loginPage)
            {
                BarBackgroundColor = Colors.Transparent,
                BarTextColor = Colors.Transparent
            };
        }

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
                    .Where(e => e.Status == "Safe" && !e.IsSynced)
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
