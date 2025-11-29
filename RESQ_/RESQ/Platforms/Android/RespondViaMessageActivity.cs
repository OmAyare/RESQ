using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.OS;

namespace RESQ.Platforms.Android
{
    [Activity(Exported = true, Name = "RESQ.Platforms.Android.RespondViaMessageActivity")]
    [IntentFilter(new[] { "android.intent.action.RESPOND_VIA_MESSAGE" },
         Categories = new[] { "android.intent.category.DEFAULT" },
         DataScheme = "smsto")]
    [IntentFilter(new[] { "android.intent.action.RESPOND_VIA_MESSAGE" },
         Categories = new[] { "android.intent.category.DEFAULT" },
         DataScheme = "sms")]
    public class RespondViaMessageActivity : Activity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Finish(); // placeholder, required only for SMS role compliance
        }
    }
}
