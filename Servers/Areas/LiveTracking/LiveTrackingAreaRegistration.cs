using System.Web.Mvc;

namespace Servers.Areas.LiveTracking
{
    public class LiveTrackingAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "LiveTracking";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                 "CreateUser",
                 "LiveTracking/Admin/User/Create",
                 new { controller = "Admin", action = "Create" }
           );

            context.MapRoute(
                 "User",
                 "LiveTracking/Admin/User",
                 new { controller = "Admin", action = "User" }
           );

            context.MapRoute(
                 "ViewProfile",
                 "LiveTracking/Admin/Profile",
                 new { controller = "Admin", action = "ViewProfile" }
           );
            context.MapRoute(
                 "ChangePassword",
                 "LiveTracking/Admin/ChangePassword",
                 new { controller = "Admin", action = "ChangePassword" }
           );
            context.MapRoute(
                "EditUser",
                "LiveTracking/Admin/User/Edit",
                new { controller = "Admin", action = "Edit" }
          );

            context.MapRoute(
                "deleteUser",
                "LiveTracking/Admin/User/Delete",
                new { controller = "Admin", action = "Delete" }
          );

            context.MapRoute(
                 "Create_Role",
                 "LiveTracking/Admin/Role/Create",
                 new { controller = "Admin", action = "Create_Role" }
           );

            context.MapRoute(
            name: "Dashboard",
            url: "LiveTracking/Admin/Dashboard",
            defaults: new { controller = "Admin", action = "Dashboard" }
        );

            context.MapRoute(
                "LiveTracking_default",
                "LiveTracking/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}