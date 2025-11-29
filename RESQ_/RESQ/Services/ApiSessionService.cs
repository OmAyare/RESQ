using System;
using System.Threading.Tasks;
using RESQ.Models;
using RESQ_API.Client;
using RESQ_API.Client.Models.RESQApi_Models;

namespace RESQ.Services
{
    public class ApiSessionService
    {
        private readonly RESQApiClientService _api;
        private const string KEY = "ActiveSessionId";

        public ApiSessionService(RESQApiClientService api)
        {
            _api = api;
        }

        public Guid? GetSession()
        {
            var stored = Preferences.Get(KEY, "");
            return Guid.TryParse(stored, out var id) ? id : null;
        }

        public void SaveSession(Guid sessionId)
        {
            Preferences.Set(KEY, sessionId.ToString());
        }

        public void ClearSession()
        {
            Preferences.Remove(KEY);
        }

        // ------------------ MAPPING -------------------------
        private RESQ_API.Client.Models.RESQApi_Models.EmergencyEvent MapToApi(RESQ.Models.EmergencyEvent local)
        {
            return new RESQ_API.Client.Models.RESQApi_Models.EmergencyEvent
            {
                EventId = local.EventID,
                UserId = local.Cust_Id,
                Latitude = local.Latitude ?? 0,
                Longitude = local.Longitude ?? 0,
                Status = local.Status,
                SessionId = local.SessionId,
                EventDateTime = local.EventDateTime
            };
        }

        // ---------------- API WRAPPERS ----------------------

        public async Task<Guid> StartSessionAsync(RESQ.Models.EmergencyEvent ev)
        {
            var apiEvent = MapToApi(ev); // Convert before sending

            var response = await _api.StartSessionAsync(apiEvent);

            var session = response!.SessionId;
            SaveSession(session);

            return session;
        }

        public async Task UpdateAsync(Guid sessionId, RESQ.Models.EmergencyEvent ev)
        {
            var apiEvent = MapToApi(ev);
            await _api.UpdateSessionAsync(sessionId, apiEvent);
        }

        public async Task EndAsync(Guid sessionId)
        {
            await _api.EndSessionAsync(sessionId);
            ClearSession();
        }
    }
}
