using System.Web.Mvc;

namespace RESQ_Server.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Admin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                  "CreateUser",
                  "Admin/User/Create",
                  new { controller = "Admin", action = "Create" }
            );

            context.MapRoute(
                 "User",
                 "Admin/User",
                 new { controller = "Admin", action = "User" }
           );

            context.MapRoute(
                 "ViewProfile",
                 "Admin/Profile",
                 new { controller = "Admin", action = "ViewProfile" }
           );
            context.MapRoute(
                 "ChangePassword",
                 "Admin/ChangePassword",
                 new { controller = "Admin", action = "ChangePassword" }
           );
            context.MapRoute(
                 "AssignWorkDelete",
                 "Admin/Company/Delete",
                 new { controller = "Admin", action = "AssignWorkDelete" }
           );
            context.MapRoute(
                 "AssignWorkEdit",
                 "Admin/Company/Edit",
                 new { controller = "Admin", action = "AssignWorkEdit" }
           );

            context.MapRoute(
                "Company",
                "Admin/Company",
                new { controller = "Admin", action = "Company" }
          );

            context.MapRoute(
                "AssignWork",
                "Admin/Company/AssignWork",
                new { controller = "Admin", action = "AssignWork" }
          );
            context.MapRoute(
                "CreateCompany",
                "Admin/Company/Create",
                new { controller = "Admin", action = "CreateCompany" }
          );

            context.MapRoute(
                "EditUser",
                "Admin/User/Edit",
                new { controller = "Admin", action = "Edit" }
          );

            context.MapRoute(
                "deleteUser",
                "Admin/User/Delete",
                new { controller = "Admin", action = "Delete" }
          );

            context.MapRoute(
                 "Create_Role",
                 "Admin/Role/Create",
                 new { controller = "Admin", action = "Create_Role" }
           );

            context.MapRoute(
            name: "Dashboard",
            url: "Livetracking/Dashboard",
            defaults: new { controller = "Livetracking", action = "Dashboard" }
        );

            context.MapRoute(
               "Admin_default",
               "Admin/{controller}/{action}/{id}",
                new { action = "Dashboard", id = UrlParameter.Optional }
            );

        }
    }
}