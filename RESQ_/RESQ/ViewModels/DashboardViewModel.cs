using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Content;
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
        private readonly IPermissionService _permissionService;
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


        public DashboardViewModel(LocalDatabase localDb, ILocationService locationService, IEmergencyEventService eventService, IPermissionService permissionService)
        {
            _localDb = localDb;
            _locationService = locationService;
            _eventService = eventService;
            _permissionService = permissionService;

            SyncEmergencyStateFromPreferences();

            MessagingCenter.Subscribe<object>(
                   this,
                   "EmergencyTriggeredExternally",
                   _ => OnExternalEmergencyTriggered()
               );


            RequestAllPermissionsOnce();

            LoadData();

            WeakReferenceMessenger.Default.Register<DetailsUpdatedMessage>(this, async (r, m) =>
            {
                await LoadDataAsync(); // reload when edit is saved
            });

            var lastStatus = Preferences.Get("EmergencyStatus", "SAFE");

            if (lastStatus == "EMERGENCY")
            {
                isSafe = false;
                StatusText = "EMERGENCY";
                StatusColor = Colors.Red;
                StartEmergencyMode(); 
            }
            var intent = new Intent(Android.Provider.Settings.ActionAccessibilitySettings);
            intent.AddFlags(ActivityFlags.NewTask);
            Android.App.Application.Context.StartActivity(intent);

        }

        private async void RequestAllPermissionsOnce()
        {
            // Do NOT show popup again if already granted
            if (Preferences.Get("PermissionsGranted", false))
                return;

            bool ok = await _permissionService.RequestAllPermissionsAsync();

            if (ok)
            {
                Preferences.Set("PermissionsGranted", true);
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Permission Needed",
                    "Please grant all permissions for full emergency features.",
                    "OK");
            }

            if (!Preferences.Get("AccessibilityOpened", false))
            {
                var intent = new Intent(Android.Provider.Settings.ActionAccessibilitySettings);
                intent.AddFlags(ActivityFlags.NewTask);
                Android.App.Application.Context.StartActivity(intent);

                Preferences.Set("AccessibilityOpened", true);
            }

            await Permissions.RequestAsync<Permissions.Phone>();

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
                    PhoneNumber = NormalizeIndianNumber(chosenNumber)
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
        public async Task GoToEventhistory()
        {
            var addeditdetailsdPage = ServiceHelper.GetService<historypage>();
            await Application.Current.MainPage.Navigation.PushAsync(addeditdetailsdPage);
        }

        [RelayCommand]
        private async Task ToggleStatusAsync()
        {
            var currentStatus = Preferences.Get("EmergencyStatus", "SAFE");

            if (currentStatus == "EMERGENCY")
            {
                // STOP emergency
                isSafe = true;
                StatusText = "SAFE";
                StatusColor = Colors.Green;

                Preferences.Set("EmergencyStatus", "SAFE");
                Preferences.Set("EmergencyActive", false);

                await _eventService.EndEmergency();
            }
            else
            {
                // START emergency
                isSafe = false;
                StatusText = "EMERGENCY";
                StatusColor = Colors.Red;

                Preferences.Set("EmergencyStatus", "EMERGENCY");
                await _eventService.TriggerEmergencyAsync();
            }
            //isSafe = !isSafe;

            //if (isSafe)
            //{
            //    //StatusText = "SAFE";
            //    //StatusColor = Colors.Green;
            //    ApplySafeUI();
            //    Preferences.Set("EmergencyStatus", "SAFE");

            //    await _eventService.EndEmergency();
            //}
            //else
            //{
            //    //StatusText = "EMERGENCY";
            //    //StatusColor = Colors.Red;
            //    ApplyEmergencyUI();
            //    Preferences.Set("EmergencyStatus", "EMERGENCY");
            //    await _eventService.TriggerEmergencyAsync();  // start session + SMS sent once
            //}
        }

        private void OnExternalEmergencyTriggered()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                isSafe = false;
                StatusText = "EMERGENCY";
                StatusColor = Colors.Red;
            });
        }

        public void SyncEmergencyStateFromPreferences()
        {
            var status = Preferences.Get("EmergencyStatus", "SAFE");

            if (status == "EMERGENCY")
            {
                isSafe = false;
                StatusText = "EMERGENCY";
                StatusColor = Colors.Red;
            }
            else
            {
                isSafe = true;
                StatusText = "SAFE";
                StatusColor = Colors.Green;
            }
        }

        private void StartEmergencyMode()
        {
            _emergencyStart = DateTime.UtcNow;

            _timer = new Timer(async _ =>
            {
                if ((DateTime.UtcNow - _emergencyStart).TotalHours >= 48)
                {
                    await StopEmergencyModeAsync();
                    return;
                }

                if (!IsEmergencyActive())
                {
                    _timer?.Dispose();
                    _timer = null;
                    return;
                }
                try
                {
                    var (lat, lng) = await _locationService.GetCurrentLocationAsync();
                    await _eventService.SendUpdateAsync(lat, lng);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"❌ Emergency update failed: {ex.Message}");
                }
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        }

        private bool IsEmergencyActive()
        {
            return Preferences.Get("EmergencyStatus", "SAFE") == "EMERGENCY";
        }

        private async Task StopEmergencyModeAsync()
        {
            _timer?.Dispose();
            _timer = null;

            await _eventService.EndEmergency();
            await _localDb.DeleteOldEmergencyEventsAsync();

            StatusText = "SAFE";
            StatusColor = Colors.Green;
            Preferences.Set("EmergencyStatus", "SAFE");
        }

        /**/
        private string NormalizeIndianNumber(string number)
        {
            number = number.Trim().Replace(" ", "").Replace("-", "");
            if (number.StartsWith("+91"))
                return number;
            if (number.StartsWith("0"))
                return "+91" + number.Substring(1);
            if (number.Length == 10)
                return "+91" + number;
            return number;
        }

        //private void RestoreState()
        //{
        //    var status = Preferences.Get("EmergencyStatus", "SAFE");

        //    if (status == "EMERGENCY")
        //        ApplyEmergencyUI();
        //    else
        //        ApplySafeUI();
        //}

        //private void ApplyEmergencyUI()
        //{
        //    isSafe = false;
        //    StatusText = "EMERGENCY";
        //    StatusColor = Colors.Red;
        //}

        //private void ApplySafeUI()
        //{
        //    isSafe = true;
        //    StatusText = "SAFE";
        //    StatusColor = Colors.Green;
        //}

        //private void SetEmergencyUIFromExternalTrigger()
        //{
        //    isSafe = false;

        //    StatusText = "EMERGENCY";
        //    StatusColor = Colors.Red;

        //    Preferences.Set("EmergencyStatus", "EMERGENCY");
        //}


        //[RelayCommand] private async Task ToggleStatusAsync() 
        //{ isSafe = !isSafe; 
        //    if (isSafe) 
        //    { StatusText = "SAFE"; StatusColor = Colors.Green; Preferences.Set("EmergencyStatus", "SAFE");
        //        StopEmergencyMode();
        //    } 
        //    else { StatusText = "EMERGENCY"; StatusColor = Colors.Red; Preferences.Set("EmergencyStatus", "EMERGENCY"); StartEmergencyMode(); }
        //}

        //private void StartEmergencyMode()
        //{ // _emergencyStart = DateTime.UtcNow;
        //   _emergencyStart = DateTime.UtcNow; 
        //    #if ANDROID 
        //    var context = Android.App.Application.Context; var intent = new Android.Content.Intent(context, typeof(RESQ.Platforms.Android.EmergencyService));
        //    if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O) 
        //        context.StartForegroundService(intent);
        //    else context.StartService(intent); 
        //    #endif //_timer = new Timer(async _ => //{ // if (DateTime.UtcNow - _emergencyStart > TimeSpan.FromHours(48)) // { // StopEmergencyMode(); // return; // } // try // { // 
        //    var (lat, lng) = await _locationService.GetCurrentLocationAsync(); 
        //    // var ev = new EmergencyEvent // { // Cust_Id = customer.Cust_Id, // from logged-in user // EventDateTime = DateTime.UtcNow, // Latitude = lat, // Longitude = lng, // Status = "EMERGENCY", // // LinkSentAt = DateTime.UtcNow // }; // await _eventService.SaveAndSendEventAsync(ev); // } // catch (Exception ex) // { // Console.WriteLine($"❌ Error in emergency loop: {ex.Message}"); // } //}, null, TimeSpan.Zero, TimeSpan.FromSeconds(30)); }
    }

    //public void DeleteContact(EmergencyContact contact)
    //{
    //    if (EmergencyContacts.Contains(contact))
    //        EmergencyContacts.Remove(contact);
    //}
}

