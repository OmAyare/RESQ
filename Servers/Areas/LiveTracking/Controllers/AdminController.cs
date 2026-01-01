using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PagedList;
using RESQ_Server.Filters;
using Servers.Areas.LiveTracking.Data;
using Servers.Models;


namespace Servers.Areas.LiveTracking.Controllers
{
    public class AdminController : Controller
    {
        private EmployeeContext db = new EmployeeContext();
        private ResqUserContext Resqdb = new ResqUserContext();
        // GET: LiveTracking/Admin
        public ActionResult Dashboard(int? year)
        {

            if (Session["UserId"] == null || (int)Session["RoleId"] != 1)
                return RedirectToAction("Index", "Home");

            int selectedYear = year ?? DateTime.Now.Year;
            var data = Resqdb.Emergency_Events
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
            ViewBag.Years = Resqdb.Emergency_Events
                .Select(e => e.EventDateTime.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToList();

            return View();
        }

        #region Users
        [AuthorizeRoles(1)] // 1 = Admin
        public ActionResult User(int? page, string searchBy, string search)
        {
            var users = db.Resqemployees.AsQueryable().ToList().ToPagedList(page ?? 1, 5);
            if (searchBy == "Username")
            {
                return View(db.Resqemployees.Where(x => x.Username.StartsWith(search) || search == null).ToList().ToPagedList(page ?? 1, 5));
            }
            else
            {
                return View(db.Resqemployees.Where(x => x.Role.Name.StartsWith(search) || search == null).ToList().ToPagedList(page ?? 1, 5));
            }
            return View(users);
        }

        [AuthorizeRoles(1)]
        public ActionResult RegisterRecord(int? page, string searchBy, string search)
        {
            var users = Resqdb.users.AsQueryable().ToList().ToPagedList(page ?? 1, 5);
            if (searchBy == "Username")
            {
                return View(Resqdb.users.Where(x => x.full_Name.StartsWith(search) || search == null).ToList().ToPagedList(page ?? 1, 5));
            }
            else if(searchBy == "Region")
            {
                return View(Resqdb.users.Where(x => x.Region.StartsWith(search) || search == null).ToList().ToPagedList(page ?? 1, 5));
            }
            else
            {
                return View(Resqdb.users.Where(x => x.District.StartsWith(search) || search == null).ToList().ToPagedList(page ?? 1, 5));
            }
            return View(users);
        }

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

            var result = query.ToPagedList(pageNumber, pageSize);

            return View(result);
        }

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

        //public ActionResult EmergencyRecord(int? page, string searchBy, string search)
        //{
        //    var emrecord = Resqdb.Emergency_Events.OrderByDescending(e => e.EventDateTime)
        //    .Select(e => new Emergencyrecords
        //    {
        //        EventId = e.id,
        //        FullName = e.user.full_Name,
        //        Region = e.user.Region,
        //        District = e.user.District,
        //        EventDateTime = e.EventDateTime
        //    }).ToList().ToPagedList(page ?? 1, 5);
        //    if (searchBy == "FullName")
        //    {
        //        return View(Resqdb.users.Where(x => x.full_Name.StartsWith(search) || search == null).ToList().ToPagedList(page ?? 1, 5));
        //    }
        //    else if (searchBy == "Region")
        //    {
        //        return View(Resqdb.users.Where(x => x.Region.StartsWith(search) || search == null).ToList().ToPagedList(page ?? 1, 5));
        //    }
        //    else
        //    {
        //        return View(Resqdb.users.Where(x => x.District.StartsWith(search) || search == null).ToList().ToPagedList(page ?? 1, 5));
        //    }
        //    return View(emrecord);
        //}

        [HttpGet]
        [Route("LiveTracking/Admin/User/Create")]
        [AuthorizeRoles(1)] // 1 = Admin
        public ActionResult Create()
        {
            ViewBag.RoleList = new SelectList(db.Roles, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRoles(1)]
        public ActionResult Create(Resqemployee user, HttpPostedFileBase UploadPhoto)
        {
            if (db.Resqemployees.Any(x => x.Username == user.Username))
                ModelState.AddModelError("Username", "This username is already in use");

            if (db.Resqemployees.Any(x => x.Email == user.Email))
                ModelState.AddModelError("Email", "This email is already in use");

            if (ModelState.IsValid)
            {
                if (UploadPhoto != null && UploadPhoto.ContentLength > 0)
                {
                    string[] allowedExt = { ".jpg", ".jpeg", ".png", ".gif" };
                    string ext = Path.GetExtension(UploadPhoto.FileName).ToLower();

                    if (!allowedExt.Contains(ext))
                    {
                        ModelState.AddModelError("UploadPhoto", "Only JPG, PNG, and GIF formats are allowed.");
                        ViewBag.RoleList = new SelectList(db.Roles, "Id", "Name", user.Role_id);
                        return View(user);
                    }

                    if (UploadPhoto.ContentLength > 2 * 1024 * 1024)
                    {
                        ModelState.AddModelError("UploadPhoto", "File size must be under 2MB.");
                        ViewBag.RoleList = new SelectList(db.Roles, "Id", "Name", user.Role_id);
                        return View(user);
                    }

                    string filename = Guid.NewGuid().ToString() + ext;
                    string path = Path.Combine(Server.MapPath("~/EmployeePhotos"), filename);
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                    UploadPhoto.SaveAs(path);

                    user.Employee_Photo = filename;
                }
                else
                {
                    user.Employee_Photo = "default.jpg";
                }

                db.Resqemployees.Add(user);
                db.SaveChanges();

                return RedirectToAction("User", new { Role_id = user.Role_id });
            }

            ViewBag.RoleList = new SelectList(db.Roles, "Id", "Name", user.Role_id);
            return View(user);
        }

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

        [HttpGet]
        [Route("Admin/User/Edit")]
        [AuthorizeRoles(1)] // 1 = Admin
        public ActionResult Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            var user = db.Resqemployees.Find(id);
            if (user == null)
                return HttpNotFound();

            ViewBag.RoleList = new SelectList(db.Roles, "Id", "Name");
            return View(user);
        }

        // POST: Admin/User/Edit/5
        [HttpPost]
        [AuthorizeRoles(1)] // 1 = Admin
        public ActionResult Edit([Bind(Include = "User_id,Username,Email,PhoneNumber,Role_id,FullName,FatherName,MotherName,Gender,DateOfBirth,JoiningDate,Branch,AadharNumber,UANNumber,BloodGroup,BankAccount_1,BankAccount_2,Employee_Photo,Password")] Resqemployee tblemployee, HttpPostedFileBase UploadPhoto)
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine("Error: " + error.ErrorMessage);
                }

                ViewBag.RoleList = new SelectList(db.Roles, "Id", "Name", tblemployee.Role_id);
                return View(tblemployee);
            }

            var userInDb = db.Resqemployees.Find(tblemployee.User_id);
            if (userInDb == null)
                return HttpNotFound();

            // Update all properties
            userInDb.Username = tblemployee.Username;
            userInDb.Email = tblemployee.Email;
            userInDb.PhoneNumber = tblemployee.PhoneNumber;
            userInDb.Role_id = tblemployee.Role_id;
            userInDb.FullName = tblemployee.FullName;
            userInDb.FatherName = tblemployee.FatherName;
            userInDb.MotherName = tblemployee.MotherName;
            userInDb.Gender = tblemployee.Gender;
            userInDb.Password = tblemployee.Password;
            userInDb.DateOfBirth = tblemployee.DateOfBirth;
            userInDb.JoiningDate = tblemployee.JoiningDate;
            userInDb.Branch = tblemployee.Branch;
            userInDb.AadharNumber = tblemployee.AadharNumber;
            userInDb.UANNumber = tblemployee.UANNumber;
            userInDb.BloodGroup = tblemployee.BloodGroup;
            userInDb.BankAccount_1 = tblemployee.BankAccount_1;
            userInDb.BankAccount_2 = tblemployee.BankAccount_2;
            if (UploadPhoto != null && UploadPhoto.ContentLength > 0)
            {
                string[] allowedExt = { ".jpg", ".jpeg", ".png", ".gif" };
                string ext = Path.GetExtension(UploadPhoto.FileName).ToLower();

                if (!allowedExt.Contains(ext))
                {
                    ModelState.AddModelError("UploadPhoto", "Only JPG, PNG, and GIF formats are allowed.");
                    ViewBag.RoleList = new SelectList(db.Roles, "Id", "Name", tblemployee.Role_id);
                    return View(tblemployee);
                }

                if (UploadPhoto.ContentLength > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError("UploadPhoto", "File size must be under 2MB.");
                    ViewBag.RoleList = new SelectList(db.Roles, "Id", "Name", tblemployee.Role_id);
                    return View(tblemployee);
                }

                string filename = Guid.NewGuid().ToString() + ext;
                string path = Path.Combine(Server.MapPath("~/EmployeePhotos"), filename);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                UploadPhoto.SaveAs(path);

                // Set new filename in the DB
                userInDb.Employee_Photo = filename;
            }

            if (ModelState.IsValid)
            {
                db.Entry(userInDb).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("user", new { Role_id = tblemployee.Role_id });
            }
            ViewBag.RoleList = new SelectList(db.Roles, "Id", "Name", tblemployee.Role_id);
            return View(tblemployee);
        }

        [HttpGet]
        [Route("Admin/User/Delete")]
        [AuthorizeRoles(1)] // 1 = Admin
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Resqemployee tblEmployee = db.Resqemployees.Find(id);
            if (tblEmployee == null)
            {
                return HttpNotFound();
            }
            return View(tblEmployee);
        }

        // POST: Admin/Admin/Delete/5
        [HttpPost]
        [AuthorizeRoles(1)] // 1 = Admin
        public ActionResult Delete(int id)
        {
            Resqemployee tblEmployee = db.Resqemployees.Find(id);
            db.Resqemployees.Remove(tblEmployee);
            db.SaveChanges();
            return RedirectToAction("User");
        }

        [HttpGet]
        [ActionName("Create_Role")]
        [Route("Admin/Role/Create")]
        [AuthorizeRoles(1)] // 1 = Admin
        public ActionResult Create_Role()
        {
            return View();
        }

        [HttpPost]
        [ActionName("Create_Role")]
        [AuthorizeRoles(1)] // 1 = Admin
        public ActionResult Create_Role(Role rol)
        {
            if (db.Roles.Any(x => x.Name == rol.Name))
            {
                ModelState.AddModelError("Name", "This role is already in use");
            }
            if (ModelState.IsValid)
            {
                db.Roles.Add(rol);
                db.SaveChanges();
                return RedirectToAction("User");
            }
            return View(rol);
        }
        #endregion
        [HttpGet]
        [AuthorizeRoles(1)] // 1 = Admin
        public JsonResult IsRoleAvailable(string Name)
        {
            return Json(!db.Roles.Any(x => x.Name == Name), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AuthorizeRoles(1)] // 1 = Admin
        public JsonResult IsUsernameAvailable(string Username)
        {
            return Json(!db.Resqemployees.Any(u => u.Username == Username), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AuthorizeRoles(1)] // 1 = Admin
        public JsonResult IsEmailAvailable(string Email)
        {
            return Json(!db.Resqemployees.Any(u => u.Email == Email), JsonRequestBehavior.AllowGet);
        }
    }
}