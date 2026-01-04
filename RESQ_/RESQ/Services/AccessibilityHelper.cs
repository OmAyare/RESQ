using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Content;
using Android.Provider;

namespace RESQ.Services
{
    public static class AccessibilityHelper
    {
        public static bool IsAccessibilityEnabled()
        {
            var context = Android.App.Application.Context;

            try
            {
                var enabledServices = Settings.Secure.GetString(
                    context.ContentResolver,
                    Settings.Secure.EnabledAccessibilityServices);

                //if (string.IsNullOrEmpty(enabledServices))
                //    return false;

                //return enabledServices.Contains(context.PackageName);
                return !string.IsNullOrEmpty(enabledServices) &&
               enabledServices.Contains(context.PackageName);
            }
            catch
            {
                return false;
            }
        }
    }
}
