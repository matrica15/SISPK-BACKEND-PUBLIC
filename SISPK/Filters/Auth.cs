using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SISPK.Models;
using System.Web.Mvc;
using System.Web.Routing;
using Newtonsoft.Json.Linq;
using System.Web.Script.Serialization;

namespace SISPK.Filters
{
    public class Auth : ActionFilterAttribute, IActionFilter
    {

        public int RoleTipe { get; set; }
        void IActionFilter.OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            SISPKEntities db = new SISPKEntities();
            var USER_ACCESS_ID = Convert.ToInt32(HttpContext.Current.Session["USER_ACCESS_ID"]);
            var USER_ID = Convert.ToInt32(HttpContext.Current.Session["USER_ID"]);
            if (USER_ID == 0)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary 
                    { 
                        { "tipe", "Apps" }, 
                        { "controller", "Auth" }, 
                        { "action", "Logout"} 
                    });
            }
            var IsRead = 0;
            var IsCreate = 0;
            var IsUpdate = 0;
            var IsDelete = 0;
            var IsApprove = 0;
            var IsPrint = 0;
            var tipe = Convert.ToString(HttpContext.Current.Request.RequestContext.RouteData.Values["tipe"]);
            var controller = Convert.ToString(HttpContext.Current.Request.RequestContext.RouteData.Values["controller"]);
            var menu_url = ("/" + tipe + "/" + controller).ToLower();
            var host = System.Web.HttpContext.Current.Request.Url.AbsolutePath;
            var menu_id = db.Database.SqlQuery<Nullable<Int32>>("SELECT CAST(MENU_ID AS INT) FROM SYS_MENU WHERE LOWER(MENU_URL) = '" + menu_url + "'").FirstOrDefault();
            filterContext.Controller.ViewBag.menu_id = menu_id;

            //return Json(new { Hasil = Query }, JsonRequestBehavior.AllowGet);

            var back_url = Convert.ToString(System.Web.HttpContext.Current.Request.ServerVariables["HTTP_REFERER"]);
            filterContext.Controller.ViewBag.menu_url = menu_url;


            var ThisAksesTrue = db.Database.SqlQuery<Nullable<Int32>>("SELECT CAST(COUNT(*) AS INT) FROM SYS_ACCESS_DETAIL T1 INNER JOIN SYS_MENU T2 ON T1.ACCESS_DETAIL_MENU_ID = T2.MENU_ID WHERE T2.MENU_ID = " + menu_id + " AND T1.ACCESS_DETAIL_ACCESS_ID = " + USER_ACCESS_ID + " AND T1.ACCESS_DETAIL_STATUS = 1 AND T1.ACCESS_DETAIL_TYPE = " + RoleTipe).FirstOrDefault();
            if (ThisAksesTrue == 0)
            {
                //filterContext.Result = new RedirectResult(back_url);
                //filterContext.Controller.ViewBag.back_url = back_url;
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary 
                    { 
                        { "tipe", "Apps" }, 
                        { "controller", "Error" },
                        { "returnurl", back_url }
                    });
            }
            var Access = db.Database.SqlQuery<Nullable<Int32>>("SELECT CAST(T1.ACCESS_DETAIL_TYPE AS INT) FROM SYS_ACCESS_DETAIL T1 INNER JOIN SYS_MENU T2 ON T1.ACCESS_DETAIL_MENU_ID = T2.MENU_ID WHERE T2.MENU_ID = " + menu_id + " AND T1.ACCESS_DETAIL_ACCESS_ID = " + USER_ACCESS_ID + " AND T1.ACCESS_DETAIL_STATUS = 1 ORDER BY T1.ACCESS_DETAIL_TYPE ASC").ToList();
            foreach (var i in Access)
            {
                if (i == 1)
                {
                    IsRead = 1;
                }
                else if (i == 2)
                {
                    IsCreate = 1;
                }
                else if (i == 3)
                {
                    IsUpdate = 1;
                }
                else if (i == 4)
                {
                    IsDelete = 1;
                }
                else if (i == 5)
                {
                    IsApprove = 1;
                }
                else if (i == 6)
                {
                    IsPrint = 1;
                }
            }
            filterContext.Controller.ViewBag.IsRead = IsRead;
            filterContext.Controller.ViewBag.IsCreate = IsCreate;
            filterContext.Controller.ViewBag.IsUpdate = IsUpdate;
            filterContext.Controller.ViewBag.IsDelete = IsDelete;
            filterContext.Controller.ViewBag.IsApprove = IsApprove;
            filterContext.Controller.ViewBag.IsPrint = IsPrint;
            filterContext.Controller.ViewBag.USER_ACCESS_ID = (USER_ACCESS_ID == 0 ? "xx" : Convert.ToString(USER_ACCESS_ID));
        }
    }
}