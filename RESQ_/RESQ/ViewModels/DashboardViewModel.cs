using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.ApplicationModel.Communication;
using RESQ.Data;
using RESQ.Messages;
using RESQ.Models;
using RESQ.Services;
using RESQ.Views;

namespace RESQ.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly ILocationService _locationService;
        private readonly IEmergencyEventService _eventService;
        private Timer? _timer;
        private DateTime _emergencyStart;
        [ObservableProperty]
        private string statusText = "SAFE";

        [ObservableProperty]
        private Color statusColor = Colors.Green;

        private bool isSafe = true;
        private readonly LocalDatabase _localDb;
        [ObservableProperty] private Customer customer;
        [ObservableProperty] private MedicalInfo medicalInfo;
        public ObservableCollection<EmergencyContact> EmergencyContacts { get; set; } = new ObservableCollection<EmergencyContact>();


        public DashboardViewModel(LocalDatabase localDb, ILocationService locationService, IEmergencyEventService eventService)
        {
            _localDb = localDb;
            _locationService = locationService;
            _eventService = eventService;

            LoadData();

            WeakReferenceMessenger.Default.Register<DetailsUpdatedMessage>(this, async (r, m) =>
            {
                await LoadDataAsync(); // reload when edit is saved
            });


        }

        private async Task LoadDataAsync()
        {
            Customer = await _localDb.GetCustomerAsync() ?? new Customer();
            MedicalInfo = await _localDb.GetMedicalInfoAsync() ?? new MedicalInfo();

            var contacts = await _localDb.GetAllEmergencyContactsAsync();

            // if no 112, add it
            if (!contacts.Any(c => c.PhoneNumber == "112"))
            {
                var default112 = new EmergencyContact
                {
                    ContactName = "Emergency",
                    PhoneNumber = "112"
                };
                await _localDb.SaveEmergencyContactAsync(default112);
                contacts.Insert(0, default112);
            }

            // refresh observable collection
            EmergencyContacts.Clear();
            foreach (var c in contacts)
                EmergencyContacts.Add(c);
        }

        private async void LoadData()
        {

            Customer = await _localDb.GetCustomerAsync() ?? new Customer();
            MedicalInfo = await _localDb.GetMedicalInfoAsync() ?? new MedicalInfo();


            var contacts = await _localDb.GetAllEmergencyContactsAsync();

            // if no 112, add it
            if (!contacts.Any(c => c.PhoneNumber == "8928393337"))
            {
                var default112 = new EmergencyContact
                {
                    ContactName = "Emergency (Test)",
                    PhoneNumber = "8928393337"
                };
                await _localDb.SaveEmergencyContactAsync(default112);
                contacts.Insert(0, default112);
            }

            // refresh observable collection
            EmergencyContacts.Clear();
            foreach (var c in contacts)
                EmergencyContacts.Add(c);
        }

        [RelayCommand]
        public async Task AddEmergencyContact()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.ContactsRead>();

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.ContactsRead>();
            }

            if (status != PermissionStatus.Granted)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Permission Required",
                    "We need access to your contacts to add an emergency contact.",
                    "OK");
                return;
            }

            try
            {
                var contact = await Microsoft.Maui.ApplicationModel.Communication.Contacts.Default.PickContactAsync();
                if (contact == null) return; // user canceled

                // If multiple phone numbers exist, let user choose
                string chosenNumber = string.Empty;

                if (contact.Phones?.Count > 1)
                {
                    var numbers = contact.Phones.Select(p => p.PhoneNumber).ToArray();
                    chosenNumber = await Application.Current.MainPage.DisplayActionSheet(
                        "Choose a number",
                        "Cancel",
                        null,
                        numbers
                    );

                    if (chosenNumber == "Cancel" || string.IsNullOrEmpty(chosenNumber))
                        return;
                }
                else
                {
                    chosenNumber = contact.Phones?.FirstOrDefault()?.PhoneNumber ?? string.Empty;
                }

                var newContact = new EmergencyContact
                {
                    ContactName = contact.DisplayName ?? contact.GivenName,
                    PhoneNumber = chosenNumber
                };

                // Prevent adding 112 again
                if (newContact.PhoneNumber == "112")
                    return;

                await _localDb.SaveEmergencyContactAsync(newContact);
                EmergencyContacts.Add(newContact);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"Could not add contact: {ex.Message}",
                    "OK");
            }
        }


        [RelayCommand]
        public async Task DeleteEmergencyContact(EmergencyContact contact)
        {
            if (contact.IsFixed112)
            {
                await Application.Current.MainPage.DisplayAlert("Notice", "112 cannot be deleted.", "OK");
                return;
            }

            await _localDb.DeleteEmergencyContactAsync(contact);
            EmergencyContacts.Remove(contact);
        }

        [RelayCommand]
        public async Task DeleteContact(EmergencyContact contact)
        {
            if (contact.IsFixed112)
            {
                await Application.Current.MainPage.DisplayAlert("Notice", "112 cannot be deleted.", "OK");
                return;
            }

            // Delete from DB
            await _localDb.DeleteEmergencyContactAsync(contact);

            // Remove from collection
            EmergencyContacts.Remove(contact);
        }

        [RelayCommand]
        public async Task GoTOEditPage()
        {
            var addeditdetailsdPage = ServiceHelper.GetService<AddEditDetailsPage>();
            await Application.Current.MainPage.Navigation.PushAsync(addeditdetailsdPage);
        }

        [RelayCommand]
        private async Task ToggleStatusAsync()
        {
            isSafe = !isSafe;

            if (isSafe)
            {
                StatusText = "SAFE";
                StatusColor = Colors.Green;
                StopEmergencyMode();
            }
            else
            {
                StatusText = "EMERGENCY";
                StatusColor = Colors.Red;
                StartEmergencyMode();
            }
        }

        private void StartEmergencyMode()
        {
            _emergencyStart = DateTime.UtcNow;

            _timer = new Timer(async _ =>
            {
                if (DateTime.UtcNow - _emergencyStart > TimeSpan.FromHours(48))
                {
                    StopEmergencyMode();
                    return;
                }

                try
                {
                    var (lat, lng) = await _locationService.GetCurrentLocationAsync();

                    var ev = new EmergencyEvent
                    {
                        Cust_Id = 1, // from logged-in user
                        EventDateTime = DateTime.UtcNow,
                        Latitude = lat,
                        Longitude = lng,
                        Status = "EMERGENCY",
                       // LinkSentAt = DateTime.UtcNow
                    };

                    await _eventService.SaveAndSendEventAsync(ev);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Error in emergency loop: {ex.Message}");
                }

            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        }

        private void StopEmergencyMode()
        {
            _timer?.Dispose();
            _timer = null;
        }
    }

    //public void DeleteContact(EmergencyContact contact)
    //{
    //    if (EmergencyContacts.Contains(contact))
    //        EmergencyContacts.Remove(contact);
    //}
}

