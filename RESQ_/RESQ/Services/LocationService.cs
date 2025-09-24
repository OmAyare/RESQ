using Microsoft.Maui.Devices.Sensors;

namespace RESQ.Services
{
    public class LocationService : ILocationService
    {
        public async Task<(double Latitude, double Longitude)> GetCurrentLocationAsync()
        {
            // Request runtime permission
            var status = await Permissions.RequestAsync<Permissions.LocationAlways>();

            if (status != PermissionStatus.Granted)
                throw new Exception("Location permission denied");

            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10));
                var location = await Geolocation.Default.GetLocationAsync(request);

                if (location != null)
                    return (location.Latitude, location.Longitude);

                throw new Exception("Unable to get location");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Location error: {ex.Message}");
                throw;
            }
        }
    }
}
