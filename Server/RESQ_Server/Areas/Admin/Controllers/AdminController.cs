using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using JobTracks.Filters;
using PagedList;
using RESQ_Server.Areas.Admin.Data;
using Role = RESQ_Server.Models.Role;
using User = RESQ_Server.Models.User;
using RESQ_Server.Models;


namespace RESQ_Server.Areas.Admin.Controllers
{
    public class AdminController : Controller
    {
        private readonly ResqEmployeeContext empdb = new ResqEmployeeContext();

        /************************************** Users ********************************************/
        #region Users
        [AuthorizeRoles(1)] // 1 = Admin
        public ActionResult User(int? page, string searchBy, string search)
        {
            var users = empdb.Users.AsQueryable().ToList().ToPagedList(page ?? 1, 5);
            if (searchBy == "Username")
            {
                return View(empdb.Users.Where(x => x.Username.StartsWith(search) || search == null).ToList().ToPagedList(page ?? 1, 5));
            }
            else
            {
                return View(empdb.Users.Where(x => x.Role.Name.StartsWith(search) || search == null).ToList().ToPagedList(page ?? 1, 5));
            }
            return View(users);
        }

        [HttpGet]
        [Route("Admin/User/Create")]
        [AuthorizeRoles(1)] // 1 = Admin
        public ActionResult Create()
        {
            ViewBag.RoleList = new SelectList(empdb.Roles, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRoles(1)]
        public ActionResult Create(User user, HttpPostedFileBase UploadPhoto)
        {
            if (empdb.Users.Any(x => x.Username == user.Username))
                ModelState.AddModelError("Username", "This username is already in use");

            if (empdb.Users.Any(x => x.Email == user.Email))
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
                        ViewBag.RoleList = new SelectList(empdb.Roles, "Id", "Name", user.Role_id);
                        return View(user);
                    }

                    if (UploadPhoto.ContentLength > 2 * 1024 * 1024)
                    {
                        ModelState.AddModelError("UploadPhoto", "File size must be under 2MB.");
                        ViewBag.RoleList = new SelectList(empdb.Roles, "Id", "Name", user.Role_id);
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

                empdb.Users.Add(user);
                empdb.SaveChanges();

                return RedirectToAction("User", new { Role_id = user.Role_id });
            }

            ViewBag.RoleList = new SelectList(empdb.Roles, "Id", "Name", user.Role_id);
            return View(user);
        }

        public ActionResult ViewProfile()
        {
            int userId = Convert.ToInt32(Session["UserId"]);
            var user = empdb.Users.Find(userId);
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ViewProfile(User model)
        {
            var user = empdb.Users.Find(model.User_id);

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

                empdb.SaveChanges();
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
            var user = empdb.Users.Find(userId);

            if (user.Password != currentPassword.Trim())
            {
                ViewBag.Error = "Old password does not match.";
                return View();
            }

            user.Password = newPassword.Trim();
            empdb.SaveChanges();
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

            var user = empdb.Users.Find(id);
            if (user == null)
                return HttpNotFound();

            ViewBag.RoleList = new SelectList(empdb.Roles, "Id", "Name");
            return View(user);
        }

        // POST: Admin/User/Edit/5
        [HttpPost]
        [AuthorizeRoles(1)] // 1 = Admin
        public ActionResult Edit([Bind(Include = "User_id,Username,Email,PhoneNumber,Role_id,FullName,FatherName,MotherName,Gender,DateOfBirth,JoiningDate,Branch,AadharNumber,UANNumber,BloodGroup,BankAccount_1,BankAccount_2,Employee_Photo,Password")] User tblemployee, HttpPostedFileBase UploadPhoto)
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine("Error: " + error.ErrorMessage);
                }

                ViewBag.RoleList = new SelectList(empdb.Roles, "Id", "Name", tblemployee.Role_id);
                return View(tblemployee);
            }

            var userInDb = empdb.Users.Find(tblemployee.User_id);
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
                    ViewBag.RoleList = new SelectList(empdb.Roles, "Id", "Name", tblemployee.Role_id);
                    return View(tblemployee);
                }

                if (UploadPhoto.ContentLength > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError("UploadPhoto", "File size must be under 2MB.");
                    ViewBag.RoleList = new SelectList(empdb.Roles, "Id", "Name", tblemployee.Role_id);
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
                empdb.Entry(userInDb).State = EntityState.Modified;
                empdb.SaveChanges();
                return RedirectToAction("user", new { Role_id = tblemployee.Role_id });
            }
            ViewBag.RoleList = new SelectList(empdb.Roles, "Id", "Name", tblemployee.Role_id);
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
            User tblEmployee = empdb.Users.Find(id);
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
            User tblEmployee = empdb.Users.Find(id);
            empdb.Users.Remove(tblEmployee);
            empdb.SaveChanges();
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
            if (empdb.Roles.Any(x => x.Name == rol.Name))
            {
                ModelState.AddModelError("Name", "This role is already in use");
            }
            if (ModelState.IsValid)
            {
                empdb.Roles.Add(rol);
                empdb.SaveChanges();
                return RedirectToAction("User");
            }
            return View(rol);
        }
        #endregion

        /************************************* Validation *******************************************/
        [HttpGet]
        [AuthorizeRoles(1)] // 1 = Admin
        public JsonResult IsRoleAvailable(string Name)
        {
            return Json(!empdb.Roles.Any(x => x.Name == Name), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AuthorizeRoles(1)] // 1 = Admin
        public JsonResult IsUsernameAvailable(string Username)
        {
            return Json(!empdb.Users.Any(u => u.Username == Username), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AuthorizeRoles(1)] // 1 = Admin
        public JsonResult IsEmailAvailable(string Email)
        {
            return Json(!empdb.Users.Any(u => u.Email == Email), JsonRequestBehavior.AllowGet);
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Index", "Home");
        }
    }
}