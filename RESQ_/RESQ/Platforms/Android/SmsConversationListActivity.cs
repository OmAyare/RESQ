using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Widget;

namespace RESQ.Platforms.Android
{
    [Activity(Exported = true, Name = "RESQ.Platforms.Android.SmsConversationListActivity")]
    public class SmsConversationListActivity : Activity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Toast.MakeText(this, "RESQ SMS Conversation Activity", ToastLength.Short).Show();
            Finish(); // invisible placeholder
        }
    }
}
