using Microsoft.Maui.ApplicationModel;
using System.Threading.Tasks;

namespace RESQ.Services
{
    public interface IPermissionService
    {
        Task<bool> RequestAllPermissionsAsync();
    }

    public class PermissionService : IPermissionService
    {
        public async Task<bool> RequestAllPermissionsAsync()
        {
            //sms 
            var status = await Permissions.CheckStatusAsync<Permissions.Sms>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.Sms>();


            // Location
            var locationStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (locationStatus != PermissionStatus.Granted)
            {
                locationStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            // Background location (optional, depends on your Phase 2 requirements)
            var backgroundStatus = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
            if (backgroundStatus != PermissionStatus.Granted)
            {
                backgroundStatus = await Permissions.RequestAsync<Permissions.LocationAlways>();
            }

            // Contacts (for SOS contacts list)
            var contactsStatus = await Permissions.CheckStatusAsync<Permissions.ContactsRead>();
            if (contactsStatus != PermissionStatus.Granted)
            {
                contactsStatus = await Permissions.RequestAsync<Permissions.ContactsRead>();
            }

            // Phone (for calling 112 or emergency number)
            var phoneStatus = await Permissions.CheckStatusAsync<Permissions.Phone>();
            if (phoneStatus != PermissionStatus.Granted)
            {
                phoneStatus = await Permissions.RequestAsync<Permissions.Phone>();
            }

            // Final decision
            return locationStatus == PermissionStatus.Granted &&
                   backgroundStatus == PermissionStatus.Granted &&
                   contactsStatus == PermissionStatus.Granted &&
                   phoneStatus == PermissionStatus.Granted &&
                   status == PermissionStatus.Granted;

        }
    }
}
