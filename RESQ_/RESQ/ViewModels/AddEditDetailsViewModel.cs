using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using RESQ.Data;
using RESQ.Messages;
using RESQ.Models;

namespace RESQ.ViewModels
{
    public partial class AddEditDetailsViewModel : ObservableObject
    {
        private readonly LocalDatabase _localDb;

        public AddEditDetailsViewModel(LocalDatabase localDb)
        {
            _localDb = localDb;
            GenderOptions = new List<string> { "Male", "Female", "Other" };

            LoadDetails();
        }

        public List<string> GenderOptions { get; }

        [ObservableProperty]private string fullName;
        [ObservableProperty]private string _gender;
        [ObservableProperty]private DateTime? _dob;
        [ObservableProperty]private string _address;
        [ObservableProperty] private double? _height;
        [ObservableProperty] private double? _weight;
        [ObservableProperty] private string _bloodType;
        [ObservableProperty] private string _ph_bloodType;
        [ObservableProperty] private bool? _organDonor;
        [ObservableProperty] private string _Remarks;
        [ObservableProperty] private string? _MedicalConditions;
        [ObservableProperty] private string? _Medications;
        [ObservableProperty] private string? _AllergiesReactions;

        private async void LoadDetails()
        {
            try
            {
                var customer = await _localDb.GetCustomerAsync();
                var medical = await _localDb.GetMedicalInfoAsync();

                if (customer != null)
                {
                    FullName = customer.FullName;
                    Gender = customer.Gender;
                    Dob = customer.DOB;
                    Address = customer.Address;
                    Height = customer.Height;
                    Weight = customer.Weight;
                    BloodType = customer.BloodType;
                    Ph_bloodType = customer.PH_BloodType;
                    OrganDonor = customer.OrganDonor;
                }

                if (medical != null)
                {
                    MedicalConditions = medical.MedicalConditions;
                    Medications = medical.Medications;
                    AllergiesReactions = medical.AllergiesReactions;
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to load details: {ex.Message}", "OK");
            }
        }


        [RelayCommand]
        private async Task SaveDetais()
        {
            try
            {
                var localCustomer = new Customer
                {
                    FullName = FullName,
                    Gender = Gender ?? "",
                    Address = Address ?? "",
                    DOB = Dob,
                    Height = Height ?? 0,
                    Weight = Weight ?? 0,
                    BloodType = BloodType ?? "",
                    PH_BloodType = Ph_bloodType ?? "",
                    OrganDonor = (bool)OrganDonor,
                    Remarks = Remarks ?? ""
                };

                var localMedicalInfo = new MedicalInfo
                {
                    MedicalConditions = MedicalConditions ?? "",
                    Medications = Medications ?? "",
                    AllergiesReactions = AllergiesReactions ?? ""
                };

                await _localDb.SaveCustomerAsync(localCustomer);
                await _localDb.SaveMedicalInfoAsync(localMedicalInfo);

                WeakReferenceMessenger.Default.Send(new DetailsUpdatedMessage());

                await Application.Current.MainPage.DisplayAlert("✅ Success", "Details saved!", "OK");

                // Go back to Dashboard
                await Application.Current.MainPage.Navigation.PopAsync();


            }
            catch(Exception ex )
            {
                await Application.Current.MainPage.DisplayAlert("❌ Error", $"Failed to register: {ex.Message}", "OK");
            }

        }





    }
}
