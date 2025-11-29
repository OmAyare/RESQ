using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Widget;
using Android.Content;
using Android.Telephony;

namespace RESQ.Platforms.Android
{
    [Service(Exported = true, Permission = "android.permission.SEND_RESPOND_VIA_MESSAGE")]
    [IntentFilter(new[] { "android.intent.action.RESPOND_VIA_MESSAGE" },
         Categories = new[] { "android.intent.category.DEFAULT" },
         DataSchemes = new[] { "sms", "smsto", "mms", "mmsto" })]
    public class HeadlessSmsSendService : Service
    {
        public override IBinder? OnBind(Intent? intent) => null;

        public override StartCommandResult OnStartCommand(Intent? intent, StartCommandFlags flags, int startId)
        {
            Toast.MakeText(this, "Headless SMS Send Service active", ToastLength.Short).Show();
            return StartCommandResult.NotSticky;
        }
    }
}
