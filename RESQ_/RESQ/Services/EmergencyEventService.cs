using RESQ.Data;
using RESQ.Models;
using RESQ_API.Client;

namespace RESQ.Services
{
    public class EmergencyEventService : IEmergencyEventService
    {
        private readonly LocalDatabase _db;
        private readonly RESQApiClientService _api;

        public EmergencyEventService(LocalDatabase db, RESQApiClientService api)
        {
            _db = db;
            _api = api;
        }

        public async Task SaveAndSendEventAsync(RESQ.Models.EmergencyEvent ev)
        {
            // Save local first
            await _db.SaveEmergencyEventAsync(ev);

            // Lookup local user to get the UserID (from MSSQL)
            var customer = await _db.GetCustomerAsync();
            if (customer == null || customer.UserID == null)
                throw new Exception("❌ No registered user found with a UserID. Cannot sync to API.");

            // Map to API model
            var apiEvent = new RESQ_API.Client.Models.RESQApi_Models.EmergencyEvent
            {
                EventId = ev.EventID,
                UserId = customer.UserID.Value, // <-- the foreign key for API
                EventDateTime = ev.EventDateTime,
                Latitude = (double)ev.Latitude,
                Longitude = (double)ev.Longitude,
                Status = ev.Status
            };

            // Send to API
            await _api.CreateEmergencyEventAsync(apiEvent);

            Console.WriteLine($"📩 SMS would be sent with location: {ev.Latitude},{ev.Longitude}");
        }

    }
}
