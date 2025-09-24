using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using PagedList;
using RESQ_Server.Areas.Admin.Data;

namespace RESQ_Server.Areas.Admin.Controllers
{
    public class AdminController : Controller
    {
        private RESQ_DBEntities _dbEntitie = new RESQ_DBEntities();

        [Route("Admin/Dashboard")]
        public ActionResult Dashboard(int? selectedYear)
        {
            return View();
        }

        [HttpGet]
        [Route("Admin/EventsDetails")]
        public ActionResult user(int? page, string searchBy, string search)
        {
            var users = _dbEntitie.users.AsQueryable().ToList().ToPagedList(page ?? 1, 5);
            if (searchBy == "Username")
            {
                return View(_dbEntitie.users.Where(x => x.UserName.StartsWith(search) || search == null).ToList().ToPagedList(page ?? 1, 5));
            }
            else
            {
                return View(_dbEntitie.Emergency_Events.Where(x => x.Status.StartsWith(search) || search == null).ToList().ToPagedList(page ?? 1, 5));
            }
            return View(users);
 
        }
    }
}