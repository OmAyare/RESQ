using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.AccessibilityServices;
using Android.App;
using Android.Util;
using Android.Views.Accessibility;

namespace RESQ.Platforms.Android
{
    [Service(Label = "AutoSendService",
               Permission = "android.permission.BIND_ACCESSIBILITY_SERVICE",
               Exported = true)]
    [IntentFilter(new[] { "android.accessibilityservice.AccessibilityService" })]
    [MetaData("android.accessibilityservice", Resource = "@xml/autosend_config")]
    public class AssistiveMessagingService : AccessibilityService
    {
        protected override void OnServiceConnected()
        {
            base.OnServiceConnected();
            Log.Info("AutoSendService", "Service connected");
        }

        public override void OnAccessibilityEvent(AccessibilityEvent e)
        {
            if (e.EventType != EventTypes.WindowContentChanged &&
                e.EventType != EventTypes.WindowStateChanged)
                return;

            var root = RootInActiveWindow;
            if (root == null) return;

            string[] sendLabels =
            {
        "Send", "SEND", "send",
        "Send SMS", "Send message",
        "→", ">", "SMS"
    };

            foreach (var label in sendLabels)
            {
                var nodes = root.FindAccessibilityNodeInfosByText(label);
                if (nodes == null) continue;

                foreach (var node in nodes)
                {
                    if (node.Clickable)
                    {
                        node.PerformAction(global::Android.Views.Accessibility.Action.Click);
                        Log.Info("AutoSendService", $"📤 Auto-clicked SEND ({label})");

                        // Close SMS app
                        PerformGlobalAction(GlobalAction.Back);
                        return;
                    }
                }
            }
        }

        public override void OnInterrupt() { }
    }
}
