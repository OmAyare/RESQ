using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace JobTracks.Filters
{
    public class AuthorizeRolesAttribute : ActionFilterAttribute
    {
        private readonly int[] allowedRoles;

        public AuthorizeRolesAttribute(params int[] roles)
        {
            this.allowedRoles = roles;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = filterContext.HttpContext.Session;

            if (session["UserId"] == null || session["RoleId"] == null)
            {
                // User not logged in
                filterContext.Result = new RedirectResult("~/Home/Index");
                return;
            }

            int currentRole = (int)session["RoleId"];

            if (!Array.Exists(allowedRoles, r => r == currentRole))
            {
                // User has no permission
                filterContext.Result = new RedirectResult("~/Home/AccessDenied"); // Optional: create this page
            }
        }
    }
}