using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Widget;

namespace RESQ.Platforms.Android
{
    [BroadcastReceiver(Enabled = true, Exported = true, Permission = "android.permission.BROADCAST_SMS")]
    [IntentFilter(new[] { "android.provider.Telephony.SMS_DELIVER" }, Categories = new[] { "android.intent.category.DEFAULT" })]
    public class SmsDeliverReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context? context, Intent? intent)
        {
            switch ((Result)ResultCode)
            {
                case Result.Ok:
                    Console.WriteLine("📬 SMS delivered to recipient!");
                    Toast.MakeText(context, "SMS delivered!", ToastLength.Short).Show();
                    break;
                default:
                    Console.WriteLine($"⚠️ SMS not delivered. Code: {ResultCode}");
                    break;
            }
        }
    }
}
