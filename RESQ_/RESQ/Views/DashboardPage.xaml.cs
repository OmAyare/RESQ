using Android.Content;
using RESQ.Models;
using RESQ.Services;
using RESQ.ViewModels;

namespace RESQ.Views;

public partial class DashboardPage : ContentPage
{
    private bool isSafe = true;
    public DashboardPage(DashboardViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
        GpsPromptService.GpsPromptRequested += OnGpsPromptRequested;
    }

    //protected override void OnAppearing()
    //{
    //    base.OnAppearing();

    //    if (BindingContext is DashboardViewModel vm)
    //    {
    //        vm.SyncEmergencyStateFromPreferences();
    //    }
    //}

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        GpsPromptService.GpsPromptRequested -= OnGpsPromptRequested;
    }

    private async void OnGpsPromptRequested()
    {
        // Must run on UI thread
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            bool turnOn = await DisplayAlert(
                "Enable Location",
                "GPS is OFF. Emergency tracking needs your location. Turn it ON?",
                "Turn On",
                "Continue Anyway");

#if ANDROID
            if (turnOn)
            {
                var intent = new Intent(Android.Provider.Settings.ActionLocationSourceSettings);
                intent.SetFlags(ActivityFlags.NewTask);
                Android.App.Application.Context.StartActivity(intent);

                // Give user ~5 seconds to enable GPS, then continue
                await Task.Delay(6000);
            }
#endif
            // Resolve the awaiting task in EmergencyEventService
            GpsPromptService.Resolve(turnOn);
        });
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is DashboardViewModel vm)
        {
            vm.SyncEmergencyStateFromPreferences();
        }

        // Show GPS popup only in SAFE mode (not during active emergency)
        bool isEmergency = Preferences.Get("EmergencyActive", false);
        if (isEmergency)
            return;

        var isGpsOff = Preferences.Get("GpsOff", false);
        if (isGpsOff)
        {
            bool turnOn = await DisplayAlert(
                "Enable Location",
                "Location is OFF. Turn it ON for accurate tracking?",
                "Turn On",
                "Continue");

#if ANDROID
            if (turnOn)
            {
                var intent = new Intent(Android.Provider.Settings.ActionLocationSourceSettings);
                intent.SetFlags(ActivityFlags.NewTask);
                Android.App.Application.Context.StartActivity(intent);
            }
#endif
            Preferences.Set("GpsOff", false);
        }
    }
    //private async void OnDeleteClicked(object sender, EventArgs e)
    //{
    //    if (sender is RippleButton rippleButton && rippleButton.BindingContext is EmergencyContact contact)
    //    {
    //        // Animate shrinking + fade before removal
    //        if (rippleButton.Parent is View cellView)
    //        {
    //            await cellView.FadeTo(0, 300, Easing.CubicIn);
    //            await cellView.ScaleTo(0.5, 300, Easing.CubicIn);
    //        }

    //        if (BindingContext is DashboardViewModel vm)
    //        {
    //            vm.DeleteContact(contact);
    //        }
    //        // Then remove from your collection
    //        //ViewModel.DeleteContact(contact);
    //    }
    //}

    //private async void OnEventsHistoryClicked(object sender, EventArgs e)
    //{
    //    // Add a "press" animation
    //    await EventsHistoryButton.ScaleTo(0.95, 100, Easing.CubicInOut);
    //    await EventsHistoryButton.ScaleTo(1.0, 100, Easing.CubicInOut);

    //    // For now, just show a message. Later, navigate to another page.
    //    await DisplayAlert("Navigation", "This will go to Events History Page", "OK");
    //}

    //private async void OnStatusButtonClicked(object sender, EventArgs e)
    //{
    //    isSafe = !isSafe;

    //    if (isSafe)
    //    {
    //        StatusButton.Text = "SAFE";
    //        StatusButton.BackgroundColor = Colors.Green;
    //    }
    //    else
    //    {
    //        StatusButton.Text = "EMERGENCY";
    //        StatusButton.BackgroundColor = Colors.Red;
    //    }

    //    // Pulse animation
    //    await StatusButton.ScaleTo(1.1, 150, Easing.CubicInOut);
    //    await StatusButton.ScaleTo(1.0, 150, Easing.CubicInOut);
    //}


}