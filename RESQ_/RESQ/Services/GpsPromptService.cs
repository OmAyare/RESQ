using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RESQ.Services
{
    public static class GpsPromptService
    {
        private static TaskCompletionSource<bool> _tcs;

        // Called by EmergencyEventService when GPS is off
        public static Task<bool> RequestGpsTurnOnAsync()
        {
            _tcs = new TaskCompletionSource<bool>();
            // Fire event so the UI layer can show the popup
            GpsPromptRequested?.Invoke();
            return _tcs.Task; // awaits until UI resolves it
        }

        // UI calls this after user responds
        public static void Resolve(bool userChoseTurnOn)
        {
            _tcs?.TrySetResult(userChoseTurnOn);
        }

        public static event Action GpsPromptRequested;
    }
}
