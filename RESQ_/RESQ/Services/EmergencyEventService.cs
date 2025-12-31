using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Telephony;
using Android.Util;
using RESQ.Data;
using RESQ.Models;
using RESQ.Platforms.Android;
using RESQ_API.Client;

namespace RESQ.Services
{
    public class EmergencyEventService : IEmergencyEventService
    {
        private readonly LocalDatabase _db;
        private readonly RESQApiClientService _api;
        private readonly ApiSessionService _sessionService;
        private static bool _sessionCreating = false;

        public EmergencyEventService(LocalDatabase db, RESQApiClientService api, ApiSessionService sessionService)
        {
            _db = db;
            _api = api;
            _sessionService = sessionService;
        }

        private RESQ_API.Client.Models.RESQApi_Models.EmergencyEvent MapToApi(EmergencyEvent local, int userId)
        {
            return new RESQ_API.Client.Models.RESQApi_Models.EmergencyEvent
            {
                //EventId = local.EventID,
                UserId = userId,
                Latitude = local.Latitude ?? 0,
                Longitude = local.Longitude ?? 0,
                Status = local.Status,
                SessionId = local.SessionId,
                EventDateTime = local.EventDateTime
            };
        }

        public async Task SendUpdateAsync(double lat, double lng)
        {
            //if (!IsEmergencyActive())
            //    return;

            var sessionId = _sessionService.GetSession();
            // if (sessionId == null) return;


            if (sessionId == null && HasInternet() && !_sessionCreating)
            {
                _sessionCreating = true;
                try
                {
                    var customeronline = await _db.GetCustomerAsync();
                    if (customeronline == null)
                        throw new Exception("❌ No user found.");

                    var location = await Geolocation.Default.GetLocationAsync(
                        new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10))
                    );

                    var evoline = new EmergencyEvent
                    {
                        Cust_Id = customeronline.Cust_Id,
                        EventDateTime = DateTime.UtcNow,
                        Status = "EMERGENCY",
                        Latitude = location?.Latitude ?? 0,
                        Longitude = location?.Longitude ?? 0
                    };

                    var apiEvent = MapToApi(evoline, customeronline.UserID.Value);

                    // Create in API + get back SessionId
                    var created = await _api.CreateEmergencyEventAsync(apiEvent);

                    evoline.SessionId = created.SessionId;

                    evoline.IsSynced = true;
                    await _db.UpdateEmergencyEventAsync(evoline);

                    _sessionService.SaveSession(created.SessionId);

                    await SendToGoogleSheet(created.SessionId, evoline.Latitude.Value, evoline.Longitude.Value);

                    // Send SMS only once
                    SendLocationToContactsAsync(created.SessionId);

                    sessionId = _sessionService.GetSession();
                }
                finally
                {
                    _sessionCreating = false;
                }
            }
            var customer = await _db.GetCustomerAsync();
            if (customer == null) return;

            var ev = new EmergencyEvent
            {
                Cust_Id = customer.Cust_Id,
                Latitude = lat,
                Longitude = lng,
                EventDateTime = DateTime.UtcNow,
                Status = "EMERGENCY",
                SessionId = sessionId ?? Guid.Empty,
                IsSynced = sessionId != null && HasInternet()
            };

            await _db.SaveEmergencyEventAsync(ev);

            if (HasInternet() && sessionId != null)
            {
                await SendToGoogleSheet(sessionId.Value, lat, lng);
                ev.IsSynced = true;

                var apiEvent = MapToApi(ev, customer.UserID.Value);
                await _api.UpdateSessionAsync(sessionId.Value, apiEvent);
            }
            else
            {
                ev.IsSynced = false;
                await SendOfflineSmsUpdate(ev, customer);
            }

        }

        public async Task EndEmergency()
        {
            var sessionId = _sessionService.GetSession();

            var customer = await _db.GetCustomerAsync();
            if (customer == null) return;

            var safeEvent = new EmergencyEvent
            {
                Cust_Id = customer.Cust_Id,
                EventDateTime = DateTime.UtcNow,
                Status = "Safe",
                IsSynced = false
            };

            await _db.SaveEmergencyEventAsync(safeEvent);

            if (HasInternet() && sessionId != null) 
            {
                try
                {
                    await _api.EndSessionAsync(sessionId.Value);
                    safeEvent.IsSynced = true;
                    await _db.UpdateEmergencyEventAsync(safeEvent);
                }
                catch 
                {
                    safeEvent.IsSynced = false;
                    await _db.UpdateEmergencyEventAsync(safeEvent);
                }
            }
            _sessionService.ClearSession();

            Preferences.Set("EmergencyStatus", "SAFE");
            Preferences.Set("EmergencyActive", false);
            //if (sessionId != null)
            //    await _api.EndSessionAsync(sessionId.Value);
            //var lastEvent = await _db.GetLastEmergencyEventAsync();
            //if (lastEvent != null)
            //{
            //    lastEvent.Status = "Safe";
            //    await _db.UpdateEmergencyEventAsync(lastEvent);
            //}
        }

        public async Task TriggerEmergencyAsync()
        {
            //if (Preferences.Get("EmergencyStatus", "SAFE") == "EMERGENCY")
            //    return;

#if ANDROID
            var context = Android.App.Application.Context;
            var intent = new Intent(context, typeof(EmergencyService));

            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
                context.StartForegroundService(intent);
            else
                context.StartService(intent);
#endif

            Preferences.Set("EmergencyActive", true);
            Preferences.Set("EmergencyStatus", "EMERGENCY");

            var customer = await _db.GetCustomerAsync();
            if (customer == null)
                throw new Exception("❌ No user found.");

            var location = await Geolocation.Default.GetLocationAsync(
                new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10))
            );

            var ev = new EmergencyEvent
            {
                Cust_Id = customer.Cust_Id,
                EventDateTime = DateTime.UtcNow,
                Status = "EMERGENCY",
                Latitude = location?.Latitude ?? 0,
                Longitude = location?.Longitude ?? 0
            };


            // Save in SQLite FIRST (same as old method)
            await _db.SaveEmergencyEventAsync(ev);

            if (HasInternet())
            {
                var apiEvent = MapToApi(ev, customer.UserID.Value);

                // Create in API + get back SessionId
                var created = await _api.CreateEmergencyEventAsync(apiEvent);

                ev.SessionId = created.SessionId;

                ev.IsSynced = true;
                await _db.UpdateEmergencyEventAsync(ev);

                _sessionService.SaveSession(created.SessionId);

                await SendToGoogleSheet(created.SessionId, ev.Latitude.Value, ev.Longitude.Value);

                // Send SMS only once
                SendLocationToContactsAsync(created.SessionId);
            }
            else
            {
                await SendOfflineSmsUpdate(ev, customer);
            }
        }

        private async Task SendOfflineSmsUpdate(EmergencyEvent ev, Customer customer)
        {
            var contacts = await _db.GetAllEmergencyContactsAsync();

            var msg =
                $"🚨 EMERGENCY ALERT (NO INTERNET)\n" +
                $"Name: {customer.FullName}\n" +
                $"Location: https://maps.google.com/?q={ev.Latitude},{ev.Longitude}\n" +
                $"Time: {DateTime.Now:HH:mm:ss}";

            var smsPermission = await Permissions.RequestAsync<Permissions.Sms>();
            if (smsPermission != PermissionStatus.Granted)
            {
                Console.WriteLine("❌ SMS permission not granted.");
                return;
            }
            var phonePermission = await Permissions.RequestAsync<Permissions.Phone>();
            if (phonePermission != PermissionStatus.Granted)
            {
                Console.WriteLine("❌ Phone permission not granted. Cannot access SIM.");
                return;
            }

            var context = Android.App.Application.Context;

            // ✅ Retrieve correct SmsManager instance for device SIM
            Android.Telephony.SmsManager smsManager;
            try
            {
                var subMgr = Android.Telephony.SubscriptionManager.From(context);

                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.LollipopMr1)
                {
                    var activeSubs = subMgr?.ActiveSubscriptionInfoList;
                    if (activeSubs != null && activeSubs.Any())
                    {
                        // Prefer the default SMS SIM if available
                        int subId = Android.Telephony.SubscriptionManager.DefaultSmsSubscriptionId;
                        if (subId == Android.Telephony.SubscriptionManager.InvalidSubscriptionId)
                            subId = activeSubs.First().SubscriptionId;

                        smsManager = Android.Telephony.SmsManager.GetSmsManagerForSubscriptionId(subId);
                        Console.WriteLine($"📡 Using SIM Subscription ID: {subId}");
                    }
                    else
                    {
                        Console.WriteLine("⚠️ No active SIM subscription detected. Using default SMS manager.");
                        smsManager = Android.Telephony.SmsManager.Default;
                    }
                }
                else
                {
                    smsManager = Android.Telephony.SmsManager.Default;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Could not get SIM info: {ex.Message}");
                smsManager = Android.Telephony.SmsManager.Default;
            }

            foreach (var contact in contacts)
            {
                if (contact.IsFixed112) continue;

                var phone = contact.PhoneNumber.Trim();
                if (!phone.StartsWith("+91") && phone.Length == 10)
                    phone = "+91" + phone;

#if ANDROID
                try
                {
                    var uri = Android.Net.Uri.Parse("smsto:" + phone);
                    var intent = new Intent(Intent.ActionSendto, uri);

                    intent.PutExtra("sms_body", msg);
                    intent.AddFlags(ActivityFlags.NewTask);

                    Android.App.Application.Context.StartActivity(intent);

                    Console.WriteLine("📩 Messages app opened");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Could not send SMS to {phone}: {ex.Message}");

                    // 🔹 Fallback for Android 12+ (open SMS app once)
                    var smsUri = Android.Net.Uri.Parse($"smsto:{phone}");
                    var intent = new Intent(Intent.ActionSendto, smsUri);
                    intent.PutExtra("sms_body", msg);
                    intent.AddFlags(ActivityFlags.NewTask);

                    Android.App.Application.Context.StartActivity(intent);

                    // After opening messages, your AccessibilityService will click SEND automatically
                    Console.WriteLine("📩 Messages app opened — auto-send will trigger.");
                }
#endif
            }
        }

        public async Task SaveAndSendEventAsync(EmergencyEvent ev)
        {
            // Save local first
            await _db.SaveEmergencyEventAsync(ev);

            // Lookup local user to get the UserID (from MSSQL)
            var customer = await _db.GetCustomerAsync();
            if (customer == null || customer.UserID == null)
                throw new Exception("❌ No registered user found with a UserID. Cannot sync to API.");

            // Map to API model
            //var apiEvent = new RESQ_API.Client.Models.RESQApi_Models.EmergencyEvent
            //{
            //    EventId = ev.EventID,
            //    UserId = customer.UserID.Value,
            //    EventDateTime = ev.EventDateTime,
            //    Latitude = (double)ev.Latitude,
            //    Longitude = (double)ev.Longitude,
            //    Status = ev.Status
            //};

            var apiEvent = new RESQ_API.Client.Models.RESQApi_Models.EmergencyEvent
            {
                //EventId = ev.EventID,
                UserId = customer.UserID.Value,
                EventDateTime = ev.EventDateTime,
                Latitude = ev.Latitude ?? 0,
                Longitude = ev.Longitude ?? 0,
                Status = ev.Status
            };

            // Send to API
            await _api.CreateEmergencyEventAsync(apiEvent);

            //            Console.WriteLine($"📩 SMS would be sent with location: {ev.Latitude},{ev.Longitude}");
            //#if ANDROID
            //            //  Step 5: Request SMS permission (first time)
            //            var smsPermission = await Permissions.RequestAsync<Permissions.Sms>();
            //            if (smsPermission != PermissionStatus.Granted)
            //            {
            //                Console.WriteLine("❌ SMS permission denied.");
            //                return;
            //            }

            // Step 6: Send SMS to all contacts except fixed emergency number (112)
            try
            {
                //SendLocationToContactsAsync();
                //    var contacts = await _db.GetAllEmergencyContactsAsync();
                //    string message =
                //        $"🚨 EMERGENCY ALERT!\n" +
                //        $"Name: {customer.FullName}\n" +
                //        $"Location: https://maps.google.com/?q={ev.Latitude},{ev.Longitude}\n" +
                //        $"Time: {DateTime.Now:HH:mm:ss}";

                //    foreach (var contact in contacts)
                //    {
                //        if (contact.IsFixed112) continue; // skip 112 (or fixed test number)

                //        try
                //        {
                //            var smsManager = SmsManager.Default;
                //            smsManager.SendTextMessage(contact.PhoneNumber, null, message, null, null);
                //            Log.Info("RESQ", $"📩 SMS sent to {contact.PhoneNumber}");
                //        }
                //        catch (Exception smsEx)
                //        {
                //            Log.Error("RESQ", $"❌ Failed to send SMS to {contact.PhoneNumber}: {smsEx.Message}");
                //        }
                //    }
            }
            catch (Exception smsMainEx)
            {
                // Log.Error("RESQ", $"❌ SMS sending failed: {smsMainEx.Message}");
            }
            //#endif
        }


        public async Task SaveAndSendEventAsync1(EmergencyEvent ev)
        {
            // 1️⃣ Save locally + send to API
            await _db.SaveEmergencyEventAsync(ev);

            var customer = await _db.GetCustomerAsync();
            if (customer == null || customer.UserID == null)
                throw new Exception("❌ No registered user found.");

            var apiEvent = new RESQ_API.Client.Models.RESQApi_Models.EmergencyEvent
            {
                UserId = customer.UserID.Value,
                EventDateTime = ev.EventDateTime,
                Latitude = ev.Latitude ?? 0,
                Longitude = ev.Longitude ?? 0,
                Status = ev.Status
            };
            await _api.CreateEmergencyEventAsync(apiEvent);

            // 2️⃣ Send SMS alerts
            var contacts = await _db.GetAllEmergencyContactsAsync();
            var msg =
                $"🚨 EMERGENCY ALERT!\n" +
                $"Name: {customer.FullName}\n" +
                $"Location: https://maps.google.com/?q={ev.Latitude},{ev.Longitude}\n" +
                $"Time: {DateTime.Now:HH:mm:ss}";

            foreach (var contact in contacts)
            {
                if (contact.IsFixed112) continue; // Skip system emergency no.

                var phone = contact.PhoneNumber.Trim();
                if (!phone.StartsWith("+91") && phone.Length == 10)
                    phone = "+91" + phone;

                try
                {
#if ANDROID
                    if (Android.OS.Build.VERSION.SdkInt < Android.OS.BuildVersionCodes.S) // Android ≤ 11
                    {
                        var sms = Android.Telephony.SmsManager.Default;
                        sms.SendTextMessage(phone, null, msg, null, null);
                        Android.Util.Log.Info("RESQ", $"✅ SMS sent via SmsManager to {phone}");
                    }
                    else // Android 12+
                    {
                        var uri = Android.Net.Uri.Parse($"smsto:{phone}");
                        var intent = new Android.Content.Intent(Android.Content.Intent.ActionSendto, uri);
                        intent.PutExtra("sms_body", msg);
                        intent.AddFlags(Android.Content.ActivityFlags.NewTask);
                        Android.App.Application.Context.StartActivity(intent);
                        Android.Util.Log.Info("RESQ", $"📩 SMS intent launched for {phone}");
                    }
#endif
                }
                catch (Exception ex)
                {
                    Android.Util.Log.Error("RESQ", $"❌ Failed to send SMS to {phone}: {ex.Message}");
                }
            }
        }

        private async void SendLocationToContactsAsync(Guid sessionId)
        {
            try
            {
                const string GoogleTrackingUrl = "https://script.google.com/macros/s/AKfycbyzFad2wyg7YphKWpwQutcOiNhmTo4OHflUIz0y6VM2ybw_eOsaPMGKhdPoSZVshX0-/exec";

                var location = await Geolocation.Default.GetLocationAsync(
                    new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10))
                );

                if (location == null)
                {
                    Console.WriteLine("⚠️ Location not found.");
                    return;
                }

                // Get all contacts
                var contacts = await _db.GetAllEmergencyContactsAsync();
                var customer = await _db.GetCustomerAsync();

                //string message = $"🚨 EMERGENCY ALERT!\n" +
                //                 $"Name: {customer.FullName}\n" +
                //                 $"Location: https://maps.google.com/?q={location.Latitude},{location.Longitude}\n" +
                //                 $"Time: {DateTime.Now:HH:mm:ss}";

                string message = $"🚨 EMERGENCY ALERT!\n" +
                                    $"Name: {customer.FullName}\n" +
                                    $"Track live location here 👇\n" +
                                    $"{GoogleTrackingUrl}?session={sessionId}\n" +
                                    $"Time: {DateTime.Now:HH:mm:ss}";


                // Request SMS permission at runtime
                var smsPermission = await Permissions.RequestAsync<Permissions.Sms>();
                if (smsPermission != PermissionStatus.Granted)
                {
                    Console.WriteLine("❌ SMS permission not granted.");
                    return;
                }
                var phonePermission = await Permissions.RequestAsync<Permissions.Phone>();
                if (phonePermission != PermissionStatus.Granted)
                {
                    Console.WriteLine("❌ Phone permission not granted. Cannot access SIM.");
                    return;
                }

                var context = Android.App.Application.Context;
                int subId = Android.Telephony.SubscriptionManager.DefaultSmsSubscriptionId; 

                // ✅ Retrieve correct SmsManager instance for device SIM
                Android.Telephony.SmsManager smsManager;
                try
                {
                    var subMgr = Android.Telephony.SubscriptionManager.From(context);

                    if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.LollipopMr1)
                    {
                        var activeSubs = subMgr?.ActiveSubscriptionInfoList;
                        if (activeSubs != null && activeSubs.Any())
                        {
                            // Prefer the default SMS SIM if available
                            subId = Android.Telephony.SubscriptionManager.DefaultSmsSubscriptionId;
                            if (subId == Android.Telephony.SubscriptionManager.InvalidSubscriptionId)
                                subId = activeSubs.First().SubscriptionId;

                            smsManager = Android.Telephony.SmsManager.GetSmsManagerForSubscriptionId(subId);
                            Console.WriteLine($"📡 Using SIM Subscription ID: {subId}");
                        }
                        else
                        {
                            Console.WriteLine("⚠️ No active SIM subscription detected. Using default SMS manager.");
                            smsManager = Android.Telephony.SmsManager.Default;
                        }
                    }
                    else
                    {
                        smsManager = Android.Telephony.SmsManager.Default;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Could not get SIM info: {ex.Message}");
                    smsManager = Android.Telephony.SmsManager.Default;
                }


                // Get correct SmsManager instance for Android 12+
                //Android.Telephony.SmsManager smsManager;

                //if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.S)
                //{
                //    // ✅ For Android 12 (API 31) and newer
                //    smsManager = Android.Telephony.SmsManager.GetSmsManagerForSubscriptionId(
                //        Android.Telephony.SubscriptionManager.DefaultSmsSubscriptionId);
                //}
                //else
                //{
                //    // ✅ For Android 11 and older
                //    smsManager = Android.Telephony.SmsManager.Default;
                //}


                foreach (var contact in contacts)
                {
                    // Skip emergency or invalid numbers
                    if (string.IsNullOrWhiteSpace(contact.PhoneNumber) || contact.IsFixed112)
                        continue;

                    var phone = contact.PhoneNumber.Trim();
                    if (!phone.StartsWith("+91") && phone.Length == 10)
                        phone = "+91" + phone;

                    //PendingIntent sentPI = null;
                    //PendingIntent deliveredPI = null;

                    //if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.S)
                    //{
                    //    sentPI = PendingIntent.GetBroadcast(context, 0, new Intent("SMS_SENT"),
                    //        PendingIntentFlags.Immutable);
                    //    deliveredPI = PendingIntent.GetBroadcast(context, 0, new Intent("SMS_DELIVERED"),
                    //        PendingIntentFlags.Immutable);
                    // }

                    try
                    {
                        //var sentIntent = new Intent("SMS_SENT");
                        //var deliveredIntent = new Intent("SMS_DELIVERED");

                        //var sentPI = PendingIntent.GetBroadcast(context, 0, sentIntent,
                        //    PendingIntentFlags.Immutable);
                        //var deliveredPI = PendingIntent.GetBroadcast(context, 0, deliveredIntent,
                        //    PendingIntentFlags.Immutable);

                        //var sentIntent = PendingIntent.GetBroadcast(context, 0, new Intent("SMS_SENT"), PendingIntentFlags.Mutable);
                        //var deliveredIntent = PendingIntent.GetBroadcast(context, 0, new Intent("SMS_DELIVERED"), PendingIntentFlags.Mutable);

                        ///*************************************************************************************************/
                        var uri = Android.Net.Uri.Parse("smsto:" + phone);
                        var intent = new Intent(Intent.ActionSendto, uri);

                        intent.PutExtra("sms_body", message);
                        intent.AddFlags(ActivityFlags.NewTask);

                        Android.App.Application.Context.StartActivity(intent);

                        Console.WriteLine("📩 Messages app opened");
                        /*************************************************************************************************/

                        //var sentIntent = new Intent(context, typeof(RESQ.Platforms.Android.SmsSentReceiver));
                        //sentIntent.SetPackage(context.PackageName);
                        //var deliveredIntent = new Intent(context, typeof(RESQ.Platforms.Android.SmsDeliverReceiver));
                        //deliveredIntent.SetPackage(context.PackageName);

                        //PendingIntent sentPI;
                        //PendingIntent deliveredPI;
                        //sentPI = PendingIntent.GetBroadcast(context, 0, sentIntent, PendingIntentFlags.Immutable);
                        //deliveredPI = PendingIntent.GetBroadcast(context, 0, deliveredIntent, PendingIntentFlags.Immutable);

                        //if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
                        //{


                        //}
                        //else
                        //{
                        //    sentPI = PendingIntent.GetBroadcast(context, 0, sentIntent, PendingIntentFlags.UpdateCurrent);
                        //    deliveredPI = PendingIntent.GetBroadcast(context, 0, deliveredIntent, PendingIntentFlags.UpdateCurrent);
                        //}

                        //smsManager.SendTextMessage(phone, null, message, sentPI, deliveredPI);
                        //  smsManager.SendTextMessage(phone, null, message, null, null);
                        // Console.WriteLine($"✅ SMS send initiated to {phone}");
                        //SmsManager.GetSmsManagerForSubscriptionId(subId).SendTextMessage(phone, null, message, null, null);


                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Could not send SMS to {phone}: {ex.Message}");

                        // 🔹 Fallback for Android 12+ (open SMS app once)
                        var smsUri = Android.Net.Uri.Parse($"smsto:{phone}");
                        var intent = new Intent(Intent.ActionSendto, smsUri);
                        intent.PutExtra("sms_body", message);
                        intent.AddFlags(ActivityFlags.NewTask);

                        Android.App.Application.Context.StartActivity(intent);

                        // After opening messages, your AccessibilityService will click SEND automatically
                        Console.WriteLine("📩 Messages app opened — auto-send will trigger.");

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SendLocationToContactsAsync error: {ex.Message}");
            }
            finally 
            {
#if ANDROID
                await Task.Delay(TimeSpan.FromSeconds(45));
                var number = "+919768135130";
                CallService.CallOnce(number);
#endif
            }
        }

        private async Task SendToGoogleSheet(Guid sessionId, double lat, double lng)
        {
            try
            {
                var url = "https://script.google.com/macros/s/AKfycbyzFad2wyg7YphKWpwQutcOiNhmTo4OHflUIz0y6VM2ybw_eOsaPMGKhdPoSZVshX0-/exec";

                var payload = new
                {
                    sessionId = sessionId.ToString(),
                    lat = lat,
                    lng = lng
                };

                var json = System.Text.Json.JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                using var client = new HttpClient();
                var response = await client.PostAsync(url, content);

                Console.WriteLine("📡 Google update: " + await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Failed to update Google Sheet: " + ex.Message);
            }
        }

        private bool HasInternet()
        {
            return Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
        }
        private bool IsEmergencyActive()
        {
            return Preferences.Get("EmergencyStatus", "SAFE") == "EMERGENCY";
        }
    }
}
