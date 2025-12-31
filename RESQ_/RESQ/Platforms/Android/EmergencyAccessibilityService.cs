// Platforms/Android/EmergencyAccessibilityService.cs
using Android.AccessibilityServices;
using Android.App;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Views.Accessibility;
using FeedbackFlags = Android.AccessibilityServices.FeedbackFlags; // for Application.Context if needed
// no 'using Microsoft.Maui.Controls' here to avoid ambiguity

namespace RESQ.Platforms.Android
{
    [Register("RESQ.Platforms.Android.EmergencyAccessibilityService")]
    [Service(
        Label = "EmergencyAccessibilityService",
        Permission = "android.permission.BIND_ACCESSIBILITY_SERVICE",
        Exported = true
    )]
    [IntentFilter(new[] { "android.accessibilityservice.AccessibilityService" })]
    public class EmergencyAccessibilityService : AccessibilityService
    {
        //protected override void OnServiceConnected()
        //{
        //    base.OnServiceConnected();
        //    Log.Info("RESQ", "Accessibility Service Connected");
        //}

        protected override void OnServiceConnected()
        {
            base.OnServiceConnected();

            var info = new AccessibilityServiceInfo
            {
                Flags = AccessibilityServiceFlags.RequestFilterKeyEvents,
                EventTypes = EventTypes.AllMask,
                FeedbackType = FeedbackFlags.Generic
            };

            SetServiceInfo(info);
            Log.Info("RESQ", "Accessibility Service Connected");
        }

        public override void OnAccessibilityEvent(AccessibilityEvent e)
        {
            // not used
        }

        public override void OnInterrupt() { }

        // IMPORTANT: match the base signature exactly (protected, nullable KeyEvent)
        //protected override bool OnKeyEvent(KeyEvent? e)
        //{
        //    // guard null
        //    if (e != null)
        //        PowerButtonDetector.OnKey(e); // implement PowerButtonDetector to detect triple press

        //    return base.OnKeyEvent(e);
        //}

        //public override void OnAccessibilityEvent(AccessibilityEvent e)
        //{
        //    if (e.EventType == EventTypes.WindowStateChanged ||
        //        e.EventType == EventTypes.WindowContentChanged)
        //    {
        //        PowerButtonDetector.OnScreenEvent();
        //    }
        //}


        protected override bool OnKeyEvent(KeyEvent e)
        {
            PowerButtonDetector.OnKey(e);
            return true;
            // return base.OnKeyEvent(e);
        }

    }

}
