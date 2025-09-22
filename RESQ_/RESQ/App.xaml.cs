using RESQ.Views;

namespace RESQ
{
    public partial class App : Application
    {
        public App(LoginPage loginPage)
        {
            InitializeComponent();

            MainPage = new NavigationPage(loginPage)
            {
                BarBackgroundColor = Colors.Transparent,
                BarTextColor = Colors.Transparent
            };
        }
    }
}
