using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using RESQ.Services;

namespace RESQ
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(
                    "resq_emergency_channel",
                    "RESQ Emergency",
                    NotificationImportance.High);

                var manager = (NotificationManager?)GetSystemService(NotificationService);
                manager?.CreateNotificationChannel(channel);
            }
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);

            if (intent?.GetBooleanExtra("TriggerEmergency", false) == true)
            {
                Preferences.Set("EmergencyStatus", "EMERGENCY");

                var svc = MauiApplication.Current.Services.GetService<IEmergencyEventService>();

                _ = svc?.TriggerEmergencyAsync();

                MessagingCenter.Send(this, "EmergencyTriggeredExternally");
            }
        }

    }
}
