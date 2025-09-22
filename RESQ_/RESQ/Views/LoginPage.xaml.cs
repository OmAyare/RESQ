using CommunityToolkit.Mvvm.Input;
using RESQ.ViewModels;

namespace RESQ.Views;

public partial class LoginPage : ContentPage
{
	public LoginPage(LoginViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}