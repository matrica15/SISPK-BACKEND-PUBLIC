using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SISPK.Filters;

namespace SISPK.Controllers.Apps
{
    public class ErrorController : Controller
    {
        [AllowAnonymous]
        public ActionResult Index(string returnurl = "")
        {
            ViewData["returnurl"] = returnurl;
            var USER_ID = Convert.ToInt32(Session["USER_ID"]);

            if (USER_ID == 0)
            {
                //return RedirectToAction("Logout", "Auth", new { tipe = "Logout" });
                return Redirect("/Apps/Auth/Logout");

            }
            else
            {
                return View();
            }
            //return View();
        }
    }
}
