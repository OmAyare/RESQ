using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RESQ.Data;
using RESQ.Views;

namespace RESQ.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly RegisterPage _registerPage;
        private readonly LocalDatabase _localDb;

        public LoginViewModel(RegisterPage registerPage, LocalDatabase localDb)
        {
            _registerPage = registerPage;
            _localDb = localDb;

            if (Preferences.ContainsKey("RememberedUsername") && Preferences.ContainsKey("RememberedAt"))
            {
                var savedAt = Preferences.Get("RememberedAt", DateTime.MinValue);
                if (DateTime.UtcNow - savedAt < TimeSpan.FromDays(30))
                {
                    Username = Preferences.Get("RememberedUsername", string.Empty);
                }
                else
                {
                    Preferences.Remove("RememberedUsername");
                    Preferences.Remove("RememberedAt");
                }
            }
        }

        [ObservableProperty] private string username;
        [ObservableProperty] private string password;
        [ObservableProperty] private string errorMessage;
        [ObservableProperty] private bool isErrorVisible;

        [RelayCommand]
        private async Task Login()
        {
            try
            {
                IsErrorVisible = false;

                var user = await _localDb.ValidateLoginAsync(Username, Password);

                if (user == null)
                {
                    ErrorMessage = "Invalid username or password.";
                    IsErrorVisible = true;
                    return;
                }

                Preferences.Set("RememberedUsername", Username);
                Preferences.Set("RememberedAt", DateTime.UtcNow);

                var dashboardPage = ServiceHelper.GetService<DashboardPage>();
                Application.Current.MainPage = new NavigationPage(dashboardPage);
            }
            catch (Exception ex) 
            {
                await Application.Current.MainPage.DisplayAlert("❌ Error", $"Failed to register: {ex.Message}", "OK");
            }

        }


        [RelayCommand]
        private async Task GoToRegister()
        {
            await Application.Current.MainPage.Navigation.PushAsync(_registerPage);
        }
    }

}
