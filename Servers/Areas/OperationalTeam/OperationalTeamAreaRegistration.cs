using System.Web.Mvc;

namespace Servers.Areas.OperationalTeam
{
    public class OperationalTeamAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "OperationalTeam";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
            name: "Operation_Dashboard",
             url: "OperationalTeam/Operation/Dashboard",
             defaults: new { controller = "Operation", action = "Dashboard" }
            );

            context.MapRoute(
                "OperationalTeam_default",
                "OperationalTeam/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}