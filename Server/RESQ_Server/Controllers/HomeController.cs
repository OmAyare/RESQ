﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RESQ_Server.Models;

namespace RESQ_Server.Controllers
{
    public class HomeController : Controller
    {
        private ResqEmployeeContext db = new  ResqEmployeeContext();
        [HttpGet]
        public ActionResult Index()
        {
            return View("Index");
        }

        [HttpPost]
        public ActionResult Index(Login adlo)
        {
            if (ModelState.IsValid)
            {
                var username = adlo.Username?.Trim().ToLower();
                var password = adlo.Password?.Trim();

                var us = db.Users.ToList()
                    .SingleOrDefault(x => x.Username.Trim().ToLower() == username && x.Password.Trim() == password);

                if (us != null)
                {
                    Session["UserId"] = us.User_id;
                    Session["Username"] = us.Username;
                    Session["RoleId"] = us.Role_id;

                    Session["FullName"] = us.FullName;
                    Session["UserCode"] = us.Username;
                    Session["Email"] = us.Email;
                    if (us.Employee_Photo != null)
                    {
                        Session["PhotoPath"] = us.Employee_Photo ?? "default.jpg";

                    }
                    else
                    {
                        Session["PhotoBase64"] = null;
                    }
                    ViewBag.ShowAnimation = true;
                    ViewBag.RoleId = us.Role_id;

                    return View(adlo);
                }

                ViewBag.Error = "Invalid username or password.";
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                ViewBag.Error = string.Join("<br/>", errors);
                return View("NotFound", "Error");
            }

            return View(adlo);
        }
    }
}