namespace RESQ.Views;
using RESQ.ViewModels;
public partial class historypage : ContentPage
{
	public historypage(LocalEventHistory vm)
	{
        InitializeComponent();
        BindingContext = vm;
    }
}