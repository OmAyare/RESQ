using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;

namespace RESQ.Platforms.Android
{
    [BroadcastReceiver(Exported = true)]
    [IntentFilter(new[] { "SMS_SENT" })]
    public class SmsSentReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            switch ((Result)ResultCode)
            {
                case Result.Ok:
                    Console.WriteLine("📤 SMS sent successfully!");
                    break;
                case Result.Canceled:
                    Console.WriteLine("❌ SMS sending canceled.");
                    break;
                default:
                    Console.WriteLine($"⚠️ SMS sending failed. Code: {ResultCode}");
                    break;
            }
        }
    }
}
