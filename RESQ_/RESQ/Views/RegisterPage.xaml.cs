using RESQ.ViewModels;

namespace RESQ.Views;

public partial class RegisterPage : ContentPage
{
	public RegisterPage(RegisterViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm; 
    }
}