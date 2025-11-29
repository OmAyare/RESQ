using Microsoft.Extensions.Logging;
using RESQ.Data;
using RESQ.Services;
using RESQ.ViewModels;
using RESQ.Views;
using RESQ_API.Client;
using RESQ_API.Client.IoC;

namespace RESQ
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("Font Awesome 7 Free-Solid-900.otf", "FASolid");
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register services
            builder.Services.AddSingleton<ILocationService, LocationService>();
            builder.Services.AddSingleton<IPermissionService, PermissionService>();
            builder.Services.AddSingleton<IEmergencyEventService, EmergencyEventService>();
            builder.Services.AddSingleton<RESQApiClientService>();
            builder.Services.AddSingleton<ApiSessionService>();



            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "resqlocal.db3");
           //File.Delete(dbPath);
            builder.Services.AddSingleton(new LocalDatabase(dbPath));

#if ANDROID
          //  builder.Services.AddDemoApiClientService(x => x.ApiBaseAddress = "http://10.0.2.2:5236/");
            builder.Services.AddDemoApiClientService(x => x.ApiBaseAddress = "http://192.168.0.166:5236/");
#else
            builder.Services.AddDemoApiClientService(x => x.ApiBaseAddress = "http://localhost:5236/");
#endif
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<LoginViewModel>();

            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<RegisterViewModel>();

            builder.Services.AddTransient<DashboardPage>();
            builder.Services.AddTransient<DashboardViewModel>();

            builder.Services.AddTransient<AddEditDetailsPage>();
            builder.Services.AddTransient<AddEditDetailsViewModel>();


            builder.Services.AddTransient<Func<RegisterPage>>(sp => () => sp.GetRequiredService<RegisterPage>());

            //   builder.Services.AddTransient<MainPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
