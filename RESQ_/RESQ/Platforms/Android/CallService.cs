using Android.App;
using Android.Content;
using Uri = Android.Net.Uri;
using Application = Android.App.Application;

namespace RESQ.Platforms.Android
{
    public static class CallService
    {
        static bool _calledOnce = false;

        public static void  CallOnce(string number)
        {
            if (_calledOnce)
                return;

            var intent = new Intent(Intent.ActionCall);
            intent.SetData(Uri.Parse($"tel:{number}"));
            intent.AddFlags(ActivityFlags.NewTask);
            Application.Context.StartActivity(intent);
            _calledOnce = true;
        }
    }
}
