using RESQ.ViewModels;

namespace RESQ.Views;

public partial class EmergencyInfoPage : ContentPage
{
	public EmergencyInfoPage(EmergencyInfoViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}