using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace JobTracks.Common
{
    public class RemoteClientServerAttribute : RemoteAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            Type controller = Assembly.GetExecutingAssembly().GetTypes()
                .FirstOrDefault(type => type.Name.ToLower() == string.Format("{0}Controller", this.RouteData["controller"].ToString().ToLower()));

            if (controller != null) 
            {
               MethodInfo action =  controller.GetMethods().FirstOrDefault(method => method.Name.ToLower() == this.RouteData["action"].ToString().ToLower());
                if (action != null) 
                {
                    object instance = Activator.CreateInstance(controller);
                    object response = action.Invoke(instance, new object[] { value });
                    if (response is JsonResult) 
                    {
                        object jsonData = ((JsonResult)response).Data;
                        if (jsonData is bool) 
                        {
                            return(bool) jsonData ? ValidationResult.Success : new ValidationResult(this.ErrorMessage);
                        }
                    }
                }
            }
            return new ValidationResult(this.ErrorMessage);

        }

        public RemoteClientServerAttribute(string routeName) : base(routeName) { }
        public RemoteClientServerAttribute(string action , string controller) : base(action, controller) { }
        public RemoteClientServerAttribute(string action , string controller , string areaName) : base(action, controller , areaName) { }
    }
}