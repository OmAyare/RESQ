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
using RESQ.Views;

namespace RESQ.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly LocalDatabase _localDb;
        [ObservableProperty] private Customer customer;
        [ObservableProperty] private MedicalInfo medicalInfo;
        public ObservableCollection<EmergencyContact> EmergencyContacts { get; set; } = new ObservableCollection<EmergencyContact>();

        public DashboardViewModel(LocalDatabase localDb)
        {
            _localDb = localDb;

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

            if (Customer != null)
            {
                var fixedContact = await _localDb.GetContactByNumberAsync("112");

                if (fixedContact == null)
                {
                    var default112 = new EmergencyContact
                    {
                        ContactName = "Emergency",
                        PhoneNumber = "112"
                    };
                    await _localDb.SaveEmergencyContactAsync(default112);
                    EmergencyContacts.Insert(0, default112);
                }
            }
        }

        private async void LoadData()
        {

            Customer = await _localDb.GetCustomerAsync() ?? new Customer();
            MedicalInfo = await _localDb.GetMedicalInfoAsync() ?? new MedicalInfo();


            if (customer != null)
            {
                var fixedContact = await _localDb.GetContactByNumberAsync("112");

                if (fixedContact == null)
                {
                    var default112 = new EmergencyContact
                    {
                        ContactName = "Emergency",
                        PhoneNumber = "112"
                    };
                    await _localDb.SaveEmergencyContactAsync(default112);
                    EmergencyContacts.Insert(0, default112);
                }
            }
        }


        //[RelayCommand]
        //public async Task AddEmergencyContact()
        //{
        //    var status = await Permissions.RequestAsync<Permissions.ContactsRead>();
        //    if (status != PermissionStatus.Granted)
        //    {
        //        await Application.Current.MainPage.DisplayAlert(
        //            "Permission Required",
        //            "We need access to your contacts to add an emergency contact.",
        //            "OK");
        //        return;
        //    }

        //    try
        //    {
        //        var contact = await 
        //        if (contact == null) return; // user canceled

        //        var (name, phone) = contact.Value;

        //        var newContact = new EmergencyContact
        //        {
        //            ContactName = name,
        //            PhoneNumber = phone
        //        };

        //        // Prevent adding 112 again
        //        if (newContact.PhoneNumber == "112")
        //            return;

        //        await _localDb.SaveEmergencyContactAsync(newContact);
        //        EmergencyContacts.Add(newContact);
        //    }
        //    catch (Exception ex)
        //    {
        //        await Application.Current.MainPage.DisplayAlert(
        //            "Error",
        //            $"Could not add contact: {ex.Message}",
        //            "OK");
        //    }
        //}

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

        //public void DeleteContact(EmergencyContact contact)
        //{
        //    if (EmergencyContacts.Contains(contact))
        //        EmergencyContacts.Remove(contact);
        //}
    }
}
