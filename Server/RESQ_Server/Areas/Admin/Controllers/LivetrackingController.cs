using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using JobTracks.Filters;
using PagedList;
using RESQ_Server.Areas.Admin.Data;
using RESQ_Server.Models;


namespace RESQ_Server.Areas.Admin.Controllers
{
    public class LivetrackingController : Controller
    {
        private readonly RESQ_DBEntities _db = new RESQ_DBEntities();

        public ActionResult Dashboard(int? year)
        {

            if (Session["UserId"] == null || (int)Session["RoleId"] != 1)
                return RedirectToAction("Index", "Home");

            int selectedYear = year ?? DateTime.Now.Year;
            var data = _db.Emergency_Events
                .Where(x => x.EventDateTime.Year == selectedYear)
                .GroupBy(x => x.user.Region.Trim())
                .Select(g => new
                {
                    State = g.Key,
                    Count = g.Count()
                })
                .ToList();

            ViewBag.StateCounts = data;
            ViewBag.SelectedYear = selectedYear;
            ViewBag.Years = _db.Emergency_Events
                .Select(e => e.EventDateTime.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToList();

            return View();
        }
    }
}