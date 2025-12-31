using RESQ.Models;
using RESQ.ViewModels;

namespace RESQ.Views;

public partial class DashboardPage : ContentPage
{
    private bool isSafe = true;
    public DashboardPage(DashboardViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is DashboardViewModel vm)
        {
            vm.SyncEmergencyStateFromPreferences();
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