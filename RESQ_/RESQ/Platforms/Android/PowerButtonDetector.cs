using Android.Views;
using Android.Util;
using System;

namespace RESQ.Platforms.Android
{
    public static class PowerButtonDetector
    {
        private static int count = 0;
        private static long lastTime = 0;

        public static void OnKey(KeyEvent e)
        {
            //if (e.KeyCode != Keycode.Power) return;

            if (e.KeyCode != Keycode.VolumeUp &&    
             e.KeyCode != Keycode.VolumeDown)
                return;

            if (e.Action != KeyEventActions.Down)
                return;

            long now = Java.Lang.JavaSystem.CurrentTimeMillis();

            if (now - lastTime < 2000)
                count++;
            else
                count = 1;

            lastTime = now;

            Log.Info("RESQ", $"[Volume Trigger] Count = {count}");

            if (count >= 3) // triple press Volume buttons (any)
            {
                count = 0;
                TriggerEmergency();
            }
        }


        private static void TriggerEmergency()
        {
            Log.Info("RESQ", "🚨 Emergency Triggered!");

            // Call the actual service
            //EmergencyTriggerSender.Start();
            EmergencyBackgroundTrigger.Trigger();
        }
    }
}
