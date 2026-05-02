using System;
using Android.Accounts;
using Android.Util;
using Android.Views;

namespace RESQ.Platforms.Android
{
    public static class PowerButtonDetector
    {
        private static int count = 0;
        private static long lastTime = 0;
        private static long _lastEventTime = 0;

        private const int PRESS_WINDOW_MS = 2000;  // 2 seconds window
        private const int DEBOUNCE_MS = 100;
        private const int REQUIRED_PRESSES = 3;    // 5 rapid presses to trigger

        /// <summary>
        /// Returns TRUE if the event was consumed (emergency triggered).
        /// Returns FALSE to let the system handle it normally (volume change).
        /// </summary>
        public static bool OnKey(KeyEvent e)
        {
            if (e.KeyCode != Keycode.VolumeUp && e.KeyCode != Keycode.VolumeDown)
                return false;

            if (e.Action != KeyEventActions.Down)
                return false;

            if (e.RepeatCount > 0)
                return false;

            long now = Java.Lang.JavaSystem.CurrentTimeMillis();

            // Debounce duplicate events
            if (now - _lastEventTime < DEBOUNCE_MS)
                return false;

            _lastEventTime = now;

            long gap = now - lastTime;

            // ✅ FIXED: if gap > window, this press is TOO LATE — reset to 1
            // The sequence must restart fresh from this press
            if (gap > PRESS_WINDOW_MS)
            {
                count = 1;           // this press is press #1 of a new sequence
                lastTime = now;
                Log.Info("RESQ", $"[Volume] Reset → Press #1 (gap was {gap}ms, too slow)");
                return false;         // never trigger on first press of new sequence
            }

            // ✅ Within window — count it
            count++;
            lastTime = now;

            Log.Info("RESQ", $"[Volume] Press #{count}/{REQUIRED_PRESSES} (gap={gap}ms)");

            if (count >= REQUIRED_PRESSES)
            {
                count = 0;
                lastTime = 0;
                _lastEventTime = 0;
                Log.Info("RESQ", "🚨 Emergency Triggered!");
                TriggerEmergency();
                return true;
            }

            return false;
        }


        //public static void OnKey(KeyEvent e)
        //{
        //    //if (e.KeyCode != Keycode.Power) return;

        //    if (e.KeyCode != Keycode.VolumeUp &&    
        //     e.KeyCode != Keycode.VolumeDown)
        //        return;

        //    if (e.Action != KeyEventActions.Down)
        //        return;

        //    long now = Java.Lang.JavaSystem.CurrentTimeMillis();

        //    if (now - lastTime < 2000)
        //        count++;
        //    else
        //        count = 1;

        //    lastTime = now;

        //    Log.Info("RESQ", $"[Volume Trigger] Count = {count}");

        //    if (count >= 3) // triple press Volume buttons (any)
        //    {
        //        count = 0;
        //        TriggerEmergency();
        //    }
        //}


        private static void TriggerEmergency()
        {
            Log.Info("RESQ", "🚨 Emergency Triggered!");

            // Call the actual service
            //EmergencyTriggerSender.Start();
            EmergencyBackgroundTrigger.Trigger();
        }
    }
}
