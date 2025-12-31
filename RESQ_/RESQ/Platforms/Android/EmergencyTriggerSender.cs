// Platforms/Android/EmergencyTriggerSender.cs
using Android.Content;
using Android.App;
using System;
using Application = Android.App.Application;

namespace RESQ.Platforms.Android
{
    public static class EmergencyTriggerSender
    {
        public static void Start()
        {
            var ctx = Application.Context; // unambiguous Android context

            // Option A — works if Intent ctor supports Type in your binding:
            // var intent = new Intent(ctx, typeof(MainActivity));

            // Option B — guaranteed: pass Java.Lang.Class
            var mainActivityClass = Java.Lang.Class.FromType(typeof(RESQ.MainActivity));
            var intent = new Intent(ctx, mainActivityClass);

            intent.PutExtra("TriggerEmergency", true);
            intent.AddFlags(ActivityFlags.NewTask);
            //Task.Delay(TimeSpan.FromSeconds(35));
            // ctx.StartActivity(intent);
        }
    }
}
