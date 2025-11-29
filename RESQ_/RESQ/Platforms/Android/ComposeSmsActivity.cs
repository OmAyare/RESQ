using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Widget;
using Android.Content;

namespace RESQ.Platforms.Android
{
    [Activity(Exported = true, Name = "RESQ.Platforms.Android.ComposeSmsActivity")]
    public class ComposeSmsActivity : Activity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Toast.MakeText(this, "RESQ ready as SMS app", ToastLength.Short).Show();
            Finish(); // just closes instantly
        }
    }
}
