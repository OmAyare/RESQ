using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using RESQ.Data;
using RESQ.Models;

namespace RESQ.ViewModels
{
    public partial class LocalEventHistory : ObservableObject
    {
        private readonly LocalDatabase _db;
        public ObservableCollection<EmergencyEvent> Events { get; set; } = new ObservableCollection<EmergencyEvent>();

        public LocalEventHistory(LocalDatabase db)
        {
            _db = db;
            LoadEvents();
        }

        public async void LoadEvents()
        {
            Events.Clear();

            var list = await _db.GetEmergencyEventsAsync();

            foreach (var ev in list.OrderByDescending(x => x.EventDateTime).Where(e => e.Status == "Safe"))
                Events.Add(ev);

            //var oldEvents = list.Where(e => (DateTime.Now - e.EventDateTime).TotalDays > 365).ToList();
            //foreach (var old in oldEvents)
            //    await _db.DeleteEmergencyEventAsync(old);


        }

    }
}
