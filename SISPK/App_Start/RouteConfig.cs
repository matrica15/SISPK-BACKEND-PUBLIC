using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SISPK
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
        
            routes.MapRoute(
                name: "Login",
                url: "{tipe}/{controller}/{action}",
                defaults: new
                {
                    tipe = "apps",
                    controller = "auth",
                    action = "index"
                }
                );
            routes.MapRoute(
               name: "Datatables",
               url: "{tipe}/{controller}/{action}/{id}/{id2}",
               defaults: new { tipe = UrlParameter.Optional, controller = UrlParameter.Optional, action = UrlParameter.Optional, id = UrlParameter.Optional, id2 = UrlParameter.Optional }
           );
            routes.MapRoute(
                name: "Apps",
                url: "{tipe}/{controller}/{action}/{id}",
                defaults: new { tipe = UrlParameter.Optional, controller = UrlParameter.Optional, action = UrlParameter.Optional, id = UrlParameter.Optional }
            );
            routes.MapRoute(
               name: "Default",
               url: "Apps/Auth"
           );
        }
    }
}