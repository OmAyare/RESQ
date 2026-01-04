using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using RESQ_Server.Filters;
using Servers.Areas.LiveTracking.Data;
using Servers.Models;

namespace Servers.Areas.OperationalTeam.Controllers
{
    public class OperationController : Controller
    {
        // GET: OperationalTeam/Operation
        private EmployeeContext db = new EmployeeContext();
        private ResqUserContext Resqdb = new ResqUserContext();
        // GET: LiveTracking/Recruiter
        [AuthorizeRoles(2)]
        public ActionResult Dashboard(int? year)
        {
            if (Session["UserId"] == null || (int)Session["RoleId"] != 1)
                return RedirectToAction("Index", "Home");

            int selectedYear = year ?? DateTime.Now.Year;

            ViewBag.SelectedYear = selectedYear;
            ViewBag.Years = Resqdb.Emergency_Events
                .Select(e => e.EventDateTime.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToList();

            return View();
        }

        [AuthorizeRoles(2)]
        public JsonResult GetDashboardData(int year)
        {
            var data = Resqdb.Emergency_Events
                .Where(x => x.EventDateTime.Year == year)
                .GroupBy(x => x.user.Region.Trim())
                .Select(g => new
                {
                    State = g.Key,
                    Count = g.Count()
                })
                .ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }
        //public ActionResult Dashboard(int? year)
        //{

        //    Response.AddHeader("Refresh", "5");
        //    if (Session["UserId"] == null || (int)Session["RoleId"] != 2)
        //        return RedirectToAction("Index", "Home");

        //    int selectedYear = year ?? DateTime.Now.Year;
        //    var data = Resqdb.Emergency_Events
        //        .Where(x => x.EventDateTime.Year == selectedYear)
        //        .GroupBy(x => x.user.Region.Trim())
        //        .Select(g => new
        //        {
        //            State = g.Key,
        //            Count = g.Count()
        //        })
        //        .ToList();

        //    ViewBag.StateCounts = data;
        //    ViewBag.SelectedYear = selectedYear;
        //    ViewBag.Years = Resqdb.Emergency_Events
        //        .Select(e => e.EventDateTime.Year)
        //        .Distinct()
        //        .OrderByDescending(y => y)
        //        .ToList();

        //    return View();
        //}

        #region Users
        [AuthorizeRoles(2)]
        [Route("OperationalTeam/RegisterRecord")]
        public ActionResult RegisterRecord(int? page, string searchBy, string search)
        {
            //var users = Resqdb.users.AsQueryable().ToList().ToPagedList(page ?? 1, 5);
            //if (searchBy == "Username")
            //{
            //    return View(Resqdb.users.Where(x => x.full_Name.StartsWith(search) || search == null).ToList().ToPagedList(page ?? 1, 5));
            //}
            //else if (searchBy == "Region")
            //{
            //    return View(Resqdb.users.Where(x => x.Region.StartsWith(search) || search == null).ToList().ToPagedList(page ?? 1, 5));
            //}
            //else
            //{
            //    return View(Resqdb.users.Where(x => x.District.StartsWith(search) || search == null).ToList().ToPagedList(page ?? 1, 5));
            //}
            //return View(users);
            int pageSize = 5;
            int pageNumber = page ?? 1;

            var query = Resqdb.users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                if (searchBy == "Username")
                {
                    query = query.Where(x => x.full_Name.StartsWith(search));
                }
                else if (searchBy == "Region")
                {
                    query = query.Where(x => x.Region.StartsWith(search));
                }
                else if (searchBy == "District")
                {
                    query = query.Where(x => x.District.StartsWith(search));
                }
            }

            ViewBag.CurrentSearch = search;
            ViewBag.CurrentSearchBy = searchBy;

            return View(query
                .OrderBy(x => x.full_Name)
                .ToPagedList(pageNumber, pageSize));
        }

        [Route("OperationalTeam/EmergencyRecord")]
        public ActionResult EmergencyRecord(int? page, string searchBy, string search)
        {
            int pageSize = 5;
            int pageNumber = page ?? 1;

            var query = Resqdb.Emergency_Events
                .OrderByDescending(e => e.EventDateTime)
                .Select(e => new Emergencyrecords
                {
                    EventId = e.id,
                    FullName = e.user.full_Name,
                    Region = e.user.Region,
                    District = e.user.District,
                    EventDateTime = e.EventDateTime
                });

            if (!string.IsNullOrEmpty(search))
            {
                if (searchBy == "FullName")
                    query = query.Where(x => x.FullName.StartsWith(search));

                else if (searchBy == "Region")
                    query = query.Where(x => x.Region.StartsWith(search));

                else if (searchBy == "District")
                    query = query.Where(x => x.District.StartsWith(search));
            }
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentSearchBy = searchBy;
            var result = query.ToPagedList(pageNumber, pageSize);

            return View(result);
        }
        [Route("OperationalTeam/EmergencyRecord/Details")]
        public ActionResult Details(int id)
        {
            const string GoogleTrackingUrl = "https://script.google.com/macros/s/AKfycbyzFad2wyg7YphKWpwQutcOiNhmTo4OHflUIz0y6VM2ybw_eOsaPMGKhdPoSZVshX0-/exec";

            var ev = Resqdb.Emergency_Events
                .Where(e => e.id == id)
                .Select(e => new EmergencyDetails
                {
                    EventDateTime = e.EventDateTime,
                    Latitude = e.Latitude,
                    Longitude = e.Longitude,
                    Status = e.Status,
                    SessionId = e.SessionId,
                    FullName = e.user.full_Name,
                    UserName = e.user.UserName,
                    Region = e.user.Region,
                    District = e.user.District,
                    PersonalPhone = e.user.Personal_PhoneNumber,
                    EmergencyPhone = e.user.Emergency_PhoneNumber
                })
                .FirstOrDefault();

            if (ev == null)
                return HttpNotFound();

            ViewBag.TrackingMessage =
                $"{GoogleTrackingUrl}?session={ev.SessionId}";

            return View(ev);
        }

        [Route("OperationalTeam/ViewProfile")]
        public ActionResult ViewProfile()
        {
            int userId = Convert.ToInt32(Session["UserId"]);
            var user = db.Resqemployees.Find(userId);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ViewProfile(Resqemployee model)
        {
            var user = db.Resqemployees.Find(model.User_id);

            if (user != null)
            {
                // Update only allowed fields
                user.FullName = model.FullName;
                user.FatherName = model.FatherName;
                user.MotherName = model.MotherName;
                user.Gender = model.Gender;
                user.DateOfBirth = model.DateOfBirth;
                user.PhoneNumber = model.PhoneNumber;
                user.AadharNumber = model.AadharNumber;
                user.BloodGroup = model.BloodGroup;
                user.UANNumber = model.UANNumber;

                db.SaveChanges();
                ViewBag.Message = "Profile updated successfully!";
                return RedirectToAction("Dashboard", "Admin");
            }
            else
            {
                ViewBag.Error = "User not found!";
            }

            return View("ViewProfile", user);
        }

        [Route("OperationalTeam/ChangePassword")]
        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(string currentPassword, string newPassword)
        {
            int userId = Convert.ToInt32(Session["UserId"]);
            var user = db.Resqemployees.Find(userId);

            if (user.Password != currentPassword.Trim())
            {
                ViewBag.Error = "Old password does not match.";
                return View();
            }

            user.Password = newPassword.Trim();
            db.SaveChanges();
            ViewBag.Message = "Password updated successfully!";
            return RedirectToAction("Dashboard", "Admin");
        }
        #endregion
    }
}