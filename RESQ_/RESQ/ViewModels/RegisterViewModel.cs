using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RESQ.Data;
using RESQ.Models;
using RESQ.Views;
using RESQ_API.Client;
using RESQ_API.Client.Models.RESQApi_Models;

namespace RESQ.ViewModels
{
    public partial class RegisterViewModel : ObservableObject
    {
        private readonly LocalDatabase _localDb;
        private readonly RESQApiClientService _apiClient;
        private readonly LoginPage _loginPage;

        public RegisterViewModel(LocalDatabase localDb, RESQApiClientService apiClient)
        {
            _localDb = localDb;
            _apiClient = apiClient;
        }


        [ObservableProperty] private string fullName;
        [ObservableProperty] private string username;  
        [ObservableProperty] private string password;  
        [ObservableProperty] private string district;
        [ObservableProperty] private string region;
        [ObservableProperty] private string personalPhone;
        [ObservableProperty] private string familyPhone;

        // 🔹 Command for Register button
        [RelayCommand]
        private async Task RegisterUser()
        {
            await Application.Current.MainPage.DisplayAlert("Clicked", $"Cheked the Details Before confirm \r\n Name: {FullName}\r\n UserName: {Username}\r\n Phone Number:{PersonalPhone}\r\n Family Phone Number: {FamilyPhone}\r\n Region: {Region}\r\n District{District}\r\n", "OK");


            try
            {
                // 1️⃣ Save to API (MSSQL)
                var newUser = new User
                {
                    FullName = FullName,
                    UserName = Username,
                    PersonalPhoneNumber = $"+91{PersonalPhone}",
                    FamilyPhoneNumber = $"+91{FamilyPhone}",
                    Region = Region,
                    District = District
                };

                var registeredUser = await _apiClient.RegisterUserAsync(newUser);

                if (registeredUser != null)
                {
                    var localCustomer = new Customer
                    {
                        UserID = registeredUser.UserId,
                        Username = registeredUser.UserName,
                        Password = Password,       
                        FullName = registeredUser.FullName
                    };

                    await _localDb.SaveCustomerAsync(localCustomer);
                }

                await Application.Current.MainPage.DisplayAlert("✅ Success", "Registration completed!", "OK");
               // await Application.Current.MainPage.Navigation.PopAsync();
                await Application.Current.MainPage.Navigation.PushAsync(_loginPage);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("❌ Error", $"Failed to register: {ex.Message}", "OK");
            }
        }
    }
}
