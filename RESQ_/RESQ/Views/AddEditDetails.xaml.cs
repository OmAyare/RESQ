using RESQ.ViewModels;

namespace RESQ.Views;

public partial class AddEditDetailsPage : ContentPage
{
	public AddEditDetailsPage(AddEditDetailsViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}