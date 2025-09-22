using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RESQ.ViewModels
{
    public static class ServiceHelper
    {
        public static T GetService<T>() => Current.GetService<T>();

        public static IServiceProvider Current =>
            Application.Current?.Handler?.MauiContext?.Services
            ?? throw new InvalidOperationException("Maui services not available");
    }

}
