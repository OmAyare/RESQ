using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RESQ.Data;
using RESQ.Models;
using RESQ.Views;

namespace RESQ.ViewModels
{
    public partial class EmergencyInfoViewModel : ObservableObject
    {
        private readonly LocalDatabase _localDb; [ObservableProperty] private Customer customer; [ObservableProperty] private MedicalInfo medicalInfo; public ObservableCollection<EmergencyContact> EmergencyContacts { get; set; } = new ObservableCollection<EmergencyContact>(); public EmergencyInfoViewModel(LocalDatabase localDb) { _localDb = localDb; LoadData(); }
        [RelayCommand] public async Task GoToLoginPage()
        {
            var loginPage = ServiceHelper.GetService<LoginPage>();
            Application.Current.MainPage = new NavigationPage(loginPage);
        }
        private async void LoadData()
        {
            Customer = await _localDb.GetCustomerAsync() ?? new Customer(); MedicalInfo = await _localDb.GetMedicalInfoAsync() ?? new MedicalInfo(); 
            var contacts = await _localDb.GetAllEmergencyContactsAsync(); 
            // if no 112, add it
            if (!contacts.Any(c => c.PhoneNumber == "8928393337")) 
            { var default112 = new EmergencyContact 
            { ContactName = "Emergency (Test)",
                PhoneNumber = "8928393337" 
            }; 
                await _localDb.SaveEmergencyContactAsync(default112); contacts.Insert(0, default112); 
            }
            // refresh observable collection
            EmergencyContacts.Clear();
            foreach (var c in contacts)
                EmergencyContacts.Add(c); 
        }
    }
}
