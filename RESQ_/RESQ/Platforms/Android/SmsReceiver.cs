using Android.App;
using Android.Content;
using Android.OS;
using Android.Telephony;
using System;
using SmsMessage = Android.Telephony.SmsMessage;

namespace RESQ.Platforms.Android
{
    [BroadcastReceiver(Enabled = true, Exported = true)]
    [IntentFilter(new[] { "android.provider.Telephony.SMS_RECEIVED" })]
    public class SmsReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action != "android.provider.Telephony.SMS_RECEIVED")
                return;

            Bundle bundle = intent.Extras;
            if (bundle == null) return;

            try
            {
                var format = bundle.GetString("format"); // Added for Android 12+
                var pdus = (Java.Lang.Object[])bundle.Get("pdus");
                if (pdus == null) return;

                var msgs = new SmsMessage[pdus.Length];
                for (int i = 0; i < msgs.Length; i++)
                {
                    msgs[i] = SmsMessage.CreateFromPdu((byte[])pdus[i], format);
                    var msgBody = msgs[i].DisplayMessageBody;
                    var sender = msgs[i].DisplayOriginatingAddress;

                    Console.WriteLine($"📩 SMS from {sender}: {msgBody}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error reading SMS: {ex.Message}");
            }
        }
    }
}
